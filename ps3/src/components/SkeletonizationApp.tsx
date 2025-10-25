import { useEffect, useRef, useState } from "react"

const MAX_CANVAS_SIZE = 500

const ZhangSuenStep = (
  bin: Uint8Array,
  width: number,
  height: number,
  step: 1 | 2
) => {
  const toRemove: number[] = []

  const get = (x: number, y: number) => {
    if (x < 0 || y < 0 || x >= width || y >= height) return 0
    return bin[y * width + x]
  }

  const neighbors = (x: number, y: number) => [
    get(x, y - 1),
    get(x + 1, y - 1),
    get(x + 1, y),
    get(x + 1, y + 1),
    get(x, y + 1),
    get(x - 1, y + 1),
    get(x - 1, y),
    get(x - 1, y - 1),
  ]

  const transitions = (nbr: number[]) => {
    let t = 0
    for (let i = 0; i < nbr.length; i++)
      if (nbr[i] === 0 && nbr[(i + 1) % 8] === 1) t++
    return t
  }

  let changed = false
  for (let y = 1; y < height - 1; y++) {
    for (let x = 1; x < width - 1; x++) {
      const idx = y * width + x
      if (bin[idx] === 0) continue
      const nbr = neighbors(x, y)
      const B = nbr.reduce((a, b) => a + b, 0)
      const A = transitions(nbr)

      if (B >= 2 && B <= 6 && A === 1) {
        if (
          (step === 1 &&
            nbr[0] * nbr[2] * nbr[4] === 0 &&
            nbr[2] * nbr[4] * nbr[6] === 0) ||
          (step === 2 &&
            nbr[0] * nbr[2] * nbr[6] === 0 &&
            nbr[0] * nbr[4] * nbr[6] === 0)
        ) {
          toRemove.push(idx)
          changed = true
        }
      }
    }
  }

  toRemove.forEach((i) => (bin[i] = 0))
  return changed
}

const zhangSuen = (bin: Uint8Array, width: number, height: number) => {
  let changed = true
  while (changed) {
    changed = false
    if (ZhangSuenStep(bin, width, height, 1)) changed = true
    if (ZhangSuenStep(bin, width, height, 2)) changed = true
  }
  return bin
}

const K3M = (bin: Uint8Array, width: number, height: number) => {
  const get = (x: number, y: number) =>
    x < 0 || y < 0 || x >= width || y >= height ? 0 : bin[y * width + x]

  let changed = true
  while (changed) {
    changed = false
    const toRemove: number[] = []

    for (let y = 1; y < height - 1; y++) {
      for (let x = 1; x < width - 1; x++) {
        const idx = y * width + x
        if (bin[idx] === 0) continue

        const B =
          get(x - 1, y - 1) +
          get(x, y - 1) +
          get(x + 1, y - 1) +
          get(x + 1, y) +
          get(x + 1, y + 1) +
          get(x, y + 1) +
          get(x - 1, y + 1) +
          get(x - 1, y)

        if (B < 2 || B > 6) continue

        const nbr = [
          get(x, y - 1),
          get(x + 1, y - 1),
          get(x + 1, y),
          get(x + 1, y + 1),
          get(x, y + 1),
          get(x - 1, y + 1),
          get(x - 1, y),
          get(x - 1, y - 1),
        ]
        let A = 0
        for (let i = 0; i < 8; i++)
          if (nbr[i] === 0 && nbr[(i + 1) % 8] === 1) A++

        if (A === 1) {
          if (
            (nbr[0] * nbr[2] * nbr[4] === 0 &&
              nbr[2] * nbr[4] * nbr[6] === 0) ||
            (nbr[0] * nbr[2] * nbr[6] === 0 && nbr[0] * nbr[4] * nbr[6] === 0)
          ) {
            toRemove.push(idx)
            changed = true
          }
        }
      }
    }

    toRemove.forEach((i) => (bin[i] = 0))
  }
  return bin
}

const binarize = (imgData: ImageData) => {
  const bin = new Uint8Array(imgData.width * imgData.height)
  for (let i = 0; i < imgData.width * imgData.height; i++) {
    const idx = i * 4
    const val =
      imgData.data[idx] * 0.299 +
      imgData.data[idx + 1] * 0.587 +
      imgData.data[idx + 2] * 0.114
    bin[i] = val > 127 ? 0 : 1
  }
  return bin
}

const drawBinary = (
  bin: Uint8Array,
  width: number,
  height: number,
  canvas: HTMLCanvasElement
) => {
  const ctx = canvas.getContext("2d")
  if (!ctx) return
  const imgData = ctx.createImageData(width, height)
  for (let i = 0; i < width * height; i++) {
    const v = bin[i] ? 0 : 255
    imgData.data[i * 4] = v
    imgData.data[i * 4 + 1] = v
    imgData.data[i * 4 + 2] = v
    imgData.data[i * 4 + 3] = 255
  }
  ctx.putImageData(imgData, 0, 0)
}

type SkeletonizationType = "Zhang-Suen" | "K3M"

export const SkeletonizationApp: React.FC = () => {
  const [imageSrc, setImageSrc] = useState<string | null>(null)
  const [method, setMethod] = useState<SkeletonizationType>("Zhang-Suen")
  const canvasRef = useRef<HTMLCanvasElement | null>(null)
  const originalImageRef = useRef<HTMLImageElement | null>(null)

  const handleFile = (e: React.ChangeEvent<HTMLInputElement>) => {
    const file = e.target.files?.[0]
    if (!file) return
    const reader = new FileReader()
    reader.onload = () => setImageSrc(reader.result as string)
    reader.readAsDataURL(file)
  }

  const processImage = () => {
    if (!canvasRef.current || !imageSrc) return
    const img = new Image()
    img.src = imageSrc
    img.onload = () => {
      let { width, height } = img
      const scale = Math.min(
        MAX_CANVAS_SIZE / width,
        MAX_CANVAS_SIZE / height,
        1
      )
      width = Math.floor(width * scale)
      height = Math.floor(height * scale)
      const canvas = canvasRef.current!
      canvas.width = width
      canvas.height = height

      const ctx = canvas.getContext("2d")
      if (!ctx) return
      ctx.drawImage(img, 0, 0, width, height)
      const imgData = ctx.getImageData(0, 0, width, height)
      const bin = binarize(imgData)

      if (method === "Zhang-Suen") zhangSuen(bin, width, height)
      else K3M(bin, width, height)

      drawBinary(bin, width, height, canvas)
    }
  }

  useEffect(() => {
    if (imageSrc && originalImageRef.current) {
      const img = originalImageRef.current
      img.src = imageSrc
    }
  }, [imageSrc])

  return (
    <div style={{ padding: "20px", fontFamily: "sans-serif" }}>
      <h2 style={{ marginBottom: "10px" }}>Szkieletyzacja obrazów</h2>
      <div style={{ marginBottom: "15px" }}>
        <input type="file" onChange={handleFile} />
        <label style={{ marginLeft: "10px" }}>
          Metoda:
          <select
            value={method}
            onChange={(e) => setMethod(e.target.value as SkeletonizationType)}
            style={{ marginLeft: "6px" }}
          >
            <option value="Zhang-Suen">Zhang–Suen</option>
            <option value="K3M">K3M</option>
          </select>
        </label>
        <button
          onClick={processImage}
          style={{
            marginLeft: "10px",
            padding: "5px 12px",
            backgroundColor: "#007bff",
            color: "white",
            border: "none",
            borderRadius: "4px",
            cursor: "pointer",
          }}
        >
          Przetwórz
        </button>
      </div>

      {imageSrc && (
        <div
          style={{
            display: "flex",
            gap: "20px",
            alignItems: "flex-start",
            marginTop: "10px",
          }}
        >
          <div>
            <h3>Oryginał</h3>
            <img
              ref={originalImageRef}
              alt="original"
              style={{
                maxWidth: "300px",
                maxHeight: "300px",
                border: "1px solid #ccc",
                borderRadius: "6px",
              }}
            />
          </div>

          <div>
            <h3>Po szkieletyzacji</h3>
            <canvas
              ref={canvasRef}
              style={{
                border: "1px solid #ccc",
                borderRadius: "6px",
                maxWidth: "300px",
                maxHeight: "300px",
              }}
            />
          </div>
        </div>
      )}
    </div>
  )
}

export default SkeletonizationApp
