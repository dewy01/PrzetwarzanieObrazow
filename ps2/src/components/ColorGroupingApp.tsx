import { useEffect, useRef, useState } from "react"

interface Pixel {
  x: number
  y: number
}

const MAX_CANVAS_SIZE = 800

const ColorGroupingApp: React.FC = () => {
  const [imageSrc, setImageSrc] = useState<string | null>(null)
  const [tolerance, setTolerance] = useState(30)
  const [maxPixels, setMaxPixels] = useState(50000)
  const [globalMode, setGlobalMode] = useState(false)
  const [mask, setMask] = useState<Uint8Array | null>(null)

  const canvasRef = useRef<HTMLCanvasElement | null>(null)
  const originalImageRef = useRef<HTMLImageElement | null>(null)

  const handleFile = (e: React.ChangeEvent<HTMLInputElement>) => {
    const file = e.target.files?.[0]
    if (!file) return
    const reader = new FileReader()
    reader.onload = () => setImageSrc(reader.result as string)
    reader.readAsDataURL(file)
  }

  useEffect(() => {
    if (imageSrc && canvasRef.current && originalImageRef.current) {
      const img = new Image()
      img.src = imageSrc
      img.onload = () => {
        let { width, height } = img
        if (width > MAX_CANVAS_SIZE || height > MAX_CANVAS_SIZE) {
          const scale = Math.min(
            MAX_CANVAS_SIZE / width,
            MAX_CANVAS_SIZE / height,
            1
          )
          width = Math.floor(width * scale)
          height = Math.floor(height * scale)
        }

        const canvas = canvasRef.current
        const original = originalImageRef.current

        if (canvas) {
          canvas.width = width
          canvas.height = height
        }

        if (original) {
          original.width = width
          original.height = height
        }

        const ctx = canvas?.getContext("2d")
        if (!ctx) return
        ctx.clearRect(0, 0, width, height)

        if (original) original.src = imageSrc
        if (original) ctx.drawImage(original, 0, 0, width, height)
      }
    }
  }, [imageSrc])

  const clear = () => {
    setImageSrc(null)
    setMask(null)
  }

  const clearSelection = () => {
    if (!imageSrc || !canvasRef.current || !originalImageRef.current) return
    const canvas = canvasRef.current
    const ctx = canvas.getContext("2d")
    if (!ctx) return
    const original = originalImageRef.current
    ctx.clearRect(0, 0, canvas.width, canvas.height)
    ctx.drawImage(original, 0, 0, canvas.width, canvas.height)
    setMask(null)
  }

  const getColorAt = (imgData: ImageData, x: number, y: number) => {
    const idx = (y * imgData.width + x) * 4
    return [
      imgData.data[idx],
      imgData.data[idx + 1],
      imgData.data[idx + 2],
      imgData.data[idx + 3],
    ]
  }

  const colorDistance = (c1: number[], c2: number[]) =>
    Math.sqrt(
      (c1[0] - c2[0]) ** 2 + (c1[1] - c2[1]) ** 2 + (c1[2] - c2[2]) ** 2
    )

  const handleClick = (e: React.MouseEvent<HTMLCanvasElement>) => {
    if (!canvasRef.current || !originalImageRef.current) return
    const canvas = canvasRef.current
    const ctx = canvas.getContext("2d")
    if (!ctx) return

    ctx.clearRect(0, 0, canvas.width, canvas.height)
    ctx.drawImage(originalImageRef.current, 0, 0, canvas.width, canvas.height)

    const rect = canvas.getBoundingClientRect()
    const scaleX = canvas.width / rect.width
    const scaleY = canvas.height / rect.height

    const x = Math.floor((e.clientX - rect.left) * scaleX)
    const y = Math.floor((e.clientY - rect.top) * scaleY)

    const imgData = ctx.getImageData(0, 0, canvas.width, canvas.height)
    const targetColor = getColorAt(imgData, x, y)

    const visited = new Set<string>()
    const queue: Pixel[] = [{ x, y }]
    const newMask = new Uint8Array(canvas.width * canvas.height)
    let pixelsAdded = 0

    const add = (nx: number, ny: number) => {
      if (nx < 0 || ny < 0 || nx >= imgData.width || ny >= imgData.height)
        return
      const key = `${nx},${ny}`
      if (visited.has(key)) return
      visited.add(key)
      const color = getColorAt(imgData, nx, ny)
      if (
        colorDistance(color, targetColor) <= tolerance &&
        pixelsAdded < maxPixels
      ) {
        queue.push({ x: nx, y: ny })
        newMask[ny * imgData.width + nx] = 1
        pixelsAdded++
      }
    }

    if (globalMode) {
      for (let i = 0; i < imgData.width * imgData.height; i++) {
        const nx = i % imgData.width
        const ny = Math.floor(i / imgData.width)
        const color = getColorAt(imgData, nx, ny)
        if (colorDistance(color, targetColor) <= tolerance) {
          newMask[ny * imgData.width + nx] = 1
          pixelsAdded++
        }
      }
    } else {
      while (queue.length && pixelsAdded < maxPixels) {
        const { x, y } = queue.shift()!
        add(x + 1, y)
        add(x - 1, y)
        add(x, y + 1)
        add(x, y - 1)
      }
    }

    setMask(newMask)
    drawMask(newMask)
  }

  const drawMask = (currentMask: Uint8Array) => {
    if (!canvasRef.current || !originalImageRef.current) return
    const canvas = canvasRef.current
    const ctx = canvas.getContext("2d")
    if (!ctx) return

    ctx.clearRect(0, 0, canvas.width, canvas.height)
    ctx.drawImage(originalImageRef.current, 0, 0, canvas.width, canvas.height)

    const imgData = ctx.getImageData(0, 0, canvas.width, canvas.height)
    for (let y = 0; y < canvas.height; y++) {
      for (let x = 0; x < canvas.width; x++) {
        const idx = (y * canvas.width + x) * 4
        if (currentMask[y * canvas.width + x]) {
          imgData.data[idx] = 255
          imgData.data[idx + 1] = 0
          imgData.data[idx + 2] = 0
          imgData.data[idx + 3] = 120
        }
      }
    }
    ctx.putImageData(imgData, 0, 0)
  }

  const applyMorphology = (type: "dilate" | "erode") => {
    if (!mask || !canvasRef.current) return
    const width = canvasRef.current.width
    const height = canvasRef.current.height
    const newMask = new Uint8Array(mask)

    for (let y = 1; y < height - 1; y++) {
      for (let x = 1; x < width - 1; x++) {
        let count = 0
        for (let dy = -1; dy <= 1; dy++) {
          for (let dx = -1; dx <= 1; dx++) {
            count += mask[(y + dy) * width + (x + dx)]
          }
        }
        const i = y * width + x
        if (type === "dilate") {
          newMask[i] = count > 0 ? 1 : 0
        } else {
          newMask[i] = count === 9 ? 1 : 0
        }
      }
    }

    setMask(newMask)
    drawMask(newMask)
  }

  return (
    <div style={{ padding: "20px" }}>
      <h2>Grupowanie kolorem</h2>
      <input type="file" onChange={handleFile} />
      <button onClick={clear} style={{ marginLeft: "10px" }}>
        Wyczyść wszystko
      </button>
      <button onClick={clearSelection} style={{ marginLeft: "10px" }}>
        Wyczyść zaznaczenie
      </button>

      {imageSrc && (
        <div style={{ display: "flex", gap: "20px", marginTop: "20px" }}>
          <div>
            <h3>Oryginał</h3>
            <img
              ref={originalImageRef}
              alt="original"
              style={{ maxWidth: "300px", maxHeight: "300px" }}
            />
          </div>
          <div>
            <h3>Canvas</h3>
            <canvas
              ref={canvasRef}
              style={{
                border: "1px solid #ccc",
                cursor: "crosshair",
                maxWidth: "300px",
                maxHeight: "300px",
              }}
              onClick={handleClick}
            />
          </div>
        </div>
      )}

      {imageSrc && (
        <div style={{ marginTop: "20px" }}>
          <label>
            Zasięg tolerancji: {tolerance}
            <input
              type="range"
              min="0"
              max="100"
              value={tolerance}
              onChange={(e) => setTolerance(Number(e.target.value))}
            />
          </label>
          <br />
          <label>
            Limit pikseli: {maxPixels}
            <input
              type="number"
              value={maxPixels}
              onChange={(e) => setMaxPixels(Number(e.target.value))}
              style={{ width: "100px", marginLeft: "10px" }}
            />
          </label>
          <br />
          <label>
            <input
              type="checkbox"
              checked={globalMode}
              onChange={(e) => setGlobalMode(e.target.checked)}
            />
            Tryb globalny (Global Flood Mode)
          </label>
          <br />
          <div style={{ marginTop: "10px" }}>
            <button onClick={() => applyMorphology("dilate")}>Dylatacja</button>
            <button
              onClick={() => applyMorphology("erode")}
              style={{ marginLeft: "10px" }}
            >
              Erozja
            </button>
          </div>
        </div>
      )}
    </div>
  )
}

export default ColorGroupingApp
