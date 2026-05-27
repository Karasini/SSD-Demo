import { useEffect, useState } from 'react'
import {
  MAX_SPEAKER_DISPLAY_NAME_LENGTH,
  validateSpeakerDisplayName,
} from '../format-labeled-transcript'
import './rename-speaker-dialog.css'

type RenameSpeakerDialogProps = {
  speakerKey: string
  currentName: string
  isSaving: boolean
  errorMessage: string | null
  onCancel: () => void
  onSave: (displayName: string) => void
}

export function RenameSpeakerDialog({
  currentName,
  isSaving,
  errorMessage,
  onCancel,
  onSave,
}: RenameSpeakerDialogProps) {
  const [value, setValue] = useState(currentName)
  const [validationError, setValidationError] = useState<string | null>(null)

  useEffect(() => {
    setValue(currentName)
    setValidationError(null)
  }, [currentName])

  const handleSubmit = (event: React.FormEvent) => {
    event.preventDefault()
    const error = validateSpeakerDisplayName(value)
    if (error) {
      setValidationError(error)
      return
    }
    onSave(value.trim())
  }

  return (
    <div className="rename-dialog-backdrop" role="presentation" onClick={onCancel}>
      <div
        className="rename-dialog"
        role="dialog"
        aria-labelledby="rename-speaker-title"
        onClick={(event) => event.stopPropagation()}
      >
        <h3 id="rename-speaker-title">Rename speaker</h3>
        <p className="rename-dialog__hint">
          Current name: <strong>{currentName}</strong>
        </p>
        <form onSubmit={handleSubmit}>
          <label htmlFor="speaker-display-name">New name</label>
          <input
            id="speaker-display-name"
            type="text"
            value={value}
            maxLength={MAX_SPEAKER_DISPLAY_NAME_LENGTH}
            autoFocus
            disabled={isSaving}
            onChange={(event) => {
              setValue(event.target.value)
              setValidationError(null)
            }}
          />
          {(validationError || errorMessage) && (
            <p className="rename-dialog__error">{validationError ?? errorMessage}</p>
          )}
          <div className="rename-dialog__actions">
            <button type="button" onClick={onCancel} disabled={isSaving}>
              Cancel
            </button>
            <button type="submit" disabled={isSaving}>
              {isSaving ? 'Saving…' : 'Save'}
            </button>
          </div>
        </form>
      </div>
    </div>
  )
}
