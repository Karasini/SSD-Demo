import { useState } from 'react'
import './export-toolbar.css'

type ExportToolbarProps = {
  fileName: string
  exportText: string
}

function downloadFileName(original: string): string {
  const dot = original.lastIndexOf('.')
  const base = dot > 0 ? original.slice(0, dot) : original
  return `${base}.txt`
}

export function ExportToolbar({ fileName, exportText }: ExportToolbarProps) {
  const [message, setMessage] = useState<string | null>(null)

  const handleCopy = async () => {
    try {
      await navigator.clipboard.writeText(exportText)
      setMessage('Copied to clipboard')
    } catch {
      setMessage('Copy failed')
    }
    setTimeout(() => setMessage(null), 2500)
  }

  const handleDownload = () => {
    try {
      const blob = new Blob([exportText], { type: 'text/plain' })
      const url = URL.createObjectURL(blob)
      const link = document.createElement('a')
      link.href = url
      link.download = downloadFileName(fileName)
      link.click()
      URL.revokeObjectURL(url)
      setMessage('Download started')
    } catch {
      setMessage('Download failed')
    }
    setTimeout(() => setMessage(null), 2500)
  }

  return (
    <div className="export-toolbar">
      <button type="button" onClick={handleDownload}>
        Download
      </button>
      <button type="button" onClick={handleCopy}>
        Copy
      </button>
      {message && <span className="export-toolbar__message">{message}</span>}
    </div>
  )
}
