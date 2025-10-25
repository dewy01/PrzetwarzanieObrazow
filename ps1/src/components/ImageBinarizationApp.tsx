import { useState } from "react"

const ImageBinarizationApp: React.FC = () => {
  const [original, setOriginal] = useState<string | null>(null)
  const [processed, setProcessed] = useState<string | null>(null)

  const handleFile = (e: React.ChangeEvent<HTMLInputElement>) => {
    const file = e.target.files?.[0]
    if (!file) return
    const reader = new FileReader()
    reader.onload = () => setOriginal(reader.result as string)
    reader.readAsDataURL(file)
  }

  const clearImages = () => {
    setOriginal(null)
    setProcessed(null)
  }

  const getImageDataFromImage = (img: HTMLImageElement) => {
    const canvas = document.createElement("canvas")
    const ctx = canvas.getContext("2d")
    if (!ctx) throw new Error("Cannot get 2D context")
    canvas.width = img.width
    canvas.height = img.height
    ctx.drawImage(img, 0, 0)
    const imgData = ctx.getImageData(0, 0, img.width, img.height)
    return { canvas, ctx, imgData }
  }

  const setCanvasToProcessed = (canvas: HTMLCanvasElement) => {
    setProcessed(canvas.toDataURL())
  }

  const applyThreshold = () => {
    if (!original) return
    const img = new Image()
    img.src = original
    img.onload = () => {
      const { canvas, ctx, imgData } = getImageDataFromImage(img)
      const threshold = 128
      for (let i = 0; i < imgData.data.length; i += 4) {
        const gray =
          0.299 * imgData.data[i] +
          0.587 * imgData.data[i + 1] +
          0.114 * imgData.data[i + 2]
        const value = gray > threshold ? 255 : 0
        imgData.data[i] = imgData.data[i + 1] = imgData.data[i + 2] = value
      }
      ctx.putImageData(imgData, 0, 0)
      setCanvasToProcessed(canvas)
    }
  }

  type LocalMethod = "niblack" | "sauvola" | "phansalkar"

  const applyLocalThreshold = (method: LocalMethod) => {
    if (!original) return
    const img = new Image()
    img.src = original
    img.onload = () => {
      const { canvas, ctx, imgData } = getImageDataFromImage(img)
      const width = canvas.width
      const height = canvas.height
      const windowSize = 15
      const kValues: Record<LocalMethod, number> = {
        niblack: -0.2,
        sauvola: 0.34,
        phansalkar: 0.25,
      }

      const grayData: number[] = []
      for (let i = 0; i < imgData.data.length; i += 4) {
        const gray =
          0.299 * imgData.data[i] +
          0.587 * imgData.data[i + 1] +
          0.114 * imgData.data[i + 2]
        grayData.push(gray)
      }

      const getIndex = (x: number, y: number) => y * width + x

      for (let y = 0; y < height; y++) {
        for (let x = 0; x < width; x++) {
          let sum = 0,
            sumSq = 0,
            count = 0
          for (
            let dy = -Math.floor(windowSize / 2);
            dy <= Math.floor(windowSize / 2);
            dy++
          ) {
            for (
              let dx = -Math.floor(windowSize / 2);
              dx <= Math.floor(windowSize / 2);
              dx++
            ) {
              const nx = x + dx
              const ny = y + dy
              if (nx >= 0 && nx < width && ny >= 0 && ny < height) {
                const g = grayData[getIndex(nx, ny)]
                sum += g
                sumSq += g * g
                count++
              }
            }
          }
          const mean = sum / count
          const std = Math.sqrt(sumSq / count - mean * mean)
          let threshold = 0
          if (method === "niblack") {
            threshold = mean + kValues[method] * std
          } else if (method === "sauvola") {
            const R = 128
            threshold = mean * (1 + kValues[method] * (std / R - 1))
          } else if (method === "phansalkar") {
            const R = 128
            const p = 2
            const q = 10
            threshold =
              mean *
              (1 +
                p * Math.exp((-q * mean) / 255) +
                kValues[method] * (std / R - 1))
          }
          const value = grayData[getIndex(x, y)] > threshold ? 255 : 0
          const idx = getIndex(x, y) * 4
          imgData.data[idx] =
            imgData.data[idx + 1] =
            imgData.data[idx + 2] =
              value
        }
      }

      ctx.putImageData(imgData, 0, 0)
      setCanvasToProcessed(canvas)
    }
  }

  const applyOtsu = () => {
    if (!original) return
    const img = new Image()
    img.src = original
    img.onload = () => {
      const { canvas, ctx, imgData } = getImageDataFromImage(img)
      const grayData: number[] = []
      for (let i = 0; i < imgData.data.length; i += 4) {
        const gray = Math.round(
          0.299 * imgData.data[i] +
            0.587 * imgData.data[i + 1] +
            0.114 * imgData.data[i + 2]
        )
        grayData.push(gray)
      }

      const hist = new Array(256).fill(0)
      grayData.forEach((g) => hist[g]++)

      const total = grayData.length
      let sumB = 0,
        wB = 0,
        maximum = 0
      const sum1 = hist.reduce((acc, val, i) => acc + i * val, 0)
      let threshold = 0

      for (let t = 0; t < 256; t++) {
        wB += hist[t]
        if (wB === 0) continue
        const wF = total - wB
        if (wF === 0) break
        sumB += t * hist[t]
        const mB = sumB / wB
        const mF = (sum1 - sumB) / wF
        const between = wB * wF * (mB - mF) * (mB - mF)
        if (between > maximum) {
          maximum = between
          threshold = t
        }
      }

      for (let i = 0; i < grayData.length; i++) {
        const value = grayData[i] > threshold ? 255 : 0
        imgData.data[i * 4] =
          imgData.data[i * 4 + 1] =
          imgData.data[i * 4 + 2] =
            value
      }

      ctx.putImageData(imgData, 0, 0)
      setCanvasToProcessed(canvas)
    }
  }

  return (
    <div style={{ padding: "20px" }}>
      <h2>Binaryzacja obrazów</h2>
      <input type="file" onChange={handleFile} />
      <button onClick={clearImages} style={{ marginLeft: "10px" }}>
        Wyczyść
      </button>

      {original && (
        <div style={{ display: "flex", marginTop: "20px", gap: "20px" }}>
          <div>
            <h3>Oryginał</h3>
            <img src={original} alt="original" style={{ maxWidth: "300px" }} />
          </div>

          <div>
            <h3>Przetworzony</h3>
            {processed ? (
              <img
                src={processed}
                alt="processed"
                style={{ maxWidth: "300px" }}
              />
            ) : (
              <div
                style={{
                  width: "300px",
                  height: "300px",
                  border: "1px solid #ccc",
                  display: "flex",
                  alignItems: "center",
                  justifyContent: "center",
                }}
              >
                Brak
              </div>
            )}
          </div>
        </div>
      )}

      {original && (
        <div
          style={{
            marginTop: "20px",
            display: "flex",
            gap: "10px",
            flexWrap: "wrap",
          }}
        >
          <button onClick={applyThreshold}>Binaryzacja progowa</button>
          <button onClick={() => applyLocalThreshold("niblack")}>
            Niblack
          </button>
          <button onClick={() => applyLocalThreshold("sauvola")}>
            Sauvola
          </button>
          <button onClick={() => applyLocalThreshold("phansalkar")}>
            Phansalkar
          </button>
          <button onClick={applyOtsu}>Otsu</button>
        </div>
      )}
    </div>
  )
}

export default ImageBinarizationApp
