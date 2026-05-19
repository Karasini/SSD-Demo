import { useRef, useState } from 'react'
import './upload-zone.css'

const MAX_BYTES = 2_147_483_648
const ALLOWED_EXTENSIONS = [
  '.mp3', '.wav', '.m4a', '.aac', '.ogg', '.flac',
  '.mp4', '.mov', '.webm', '.mkv', '.avi',
]

type UploadZoneProps = {
  onStart: (file: File) => void
  isUploading: boolean
  uploadProgress: number
  error: string | null
}

function validateFile(file: File): string | null {
  const ext = file.name.includes('.')
    ? file.name.slice(file.name.lastIndexOf('.')).toLowerCase()
    : ''
  if (!ALLOWED_EXTENSIONS.includes(ext)) {
    return 'Unsupported file type. Allowed: mp3, wav, m4a, aac, ogg, flac, mp4, mov, webm, mkv, avi.'
  }
  if (file.size > MAX_BYTES) {
    return 'Maximum file size is 2 GB.'
  }
  return null
}

export function UploadZone({
  onStart,
  isUploading,
  uploadProgress,
  error,
}: UploadZoneProps) {
  const inputRef = useRef<HTMLInputElement>(null)
  const [selectedFile, setSelectedFile] = useState<File | null>(null)
  const [localError, setLocalError] = useState<string | null>(null)

  const handleFile = (file: File | undefined) => {
    if (!file) return
    const validationError = validateFile(file)
    if (validationError) {
      setLocalError(validationError)
      setSelectedFile(null)
      return
    }
    setLocalError(null)
    setSelectedFile(file)
  }

  const displayError = error ?? localError

  return (
    <div className="upload-zone">
      <div
        className="upload-zone__drop"
        onDragOver={(e) => e.preventDefault()}
        onDrop={(e) => {
          e.preventDefault()
          handleFile(e.dataTransfer.files[0])
        }}
      >
        <p>Drag and drop a file here, or choose a file.</p>
        <p className="upload-zone__hint">
          Max size 2 GB. Formats: MP3, WAV, M4A, AAC, OGG, FLAC, MP4, MOV, WebM, MKV, AVI.
        </p>
        <button
          type="button"
          onClick={() => inputRef.current?.click()}
          disabled={isUploading}
        >
          Choose file
        </button>
        <input
          ref={inputRef}
          type="file"
          hidden
          accept={ALLOWED_EXTENSIONS.join(',')}
          onChange={(e) => handleFile(e.target.files?.[0])}
        />
      </div>

      {selectedFile && (
        <div className="upload-zone__selected">
          <p>
            <strong>{selectedFile.name}</strong> ({(selectedFile.size / (1024 * 1024)).toFixed(1)} MB)
          </p>
          <button
            type="button"
            className="primary"
            disabled={isUploading}
            onClick={() => onStart(selectedFile)}
          >
            Start transcription
          </button>
        </div>
      )}

      {isUploading && (
        <div className="upload-zone__progress">
          <div className="upload-zone__bar" style={{ width: `${uploadProgress}%` }} />
          <span>Uploading… {uploadProgress}%</span>
        </div>
      )}

      {displayError && <p className="upload-zone__error">{displayError}</p>}
    </div>
  )
}
