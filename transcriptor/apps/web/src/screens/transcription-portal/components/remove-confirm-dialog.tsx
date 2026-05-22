import './remove-confirm-dialog.css'

export type RemoveConfirmMode = 'single' | 'bulk'

type RemoveConfirmDialogProps = {
  mode: RemoveConfirmMode
  fileName?: string
  count: number
  showProcessingNote: boolean
  isRemoving: boolean
  error: string | null
  onCancel: () => void
  onConfirm: () => void
}

export function RemoveConfirmDialog({
  mode,
  fileName,
  count,
  showProcessingNote,
  isRemoving,
  error,
  onCancel,
  onConfirm,
}: RemoveConfirmDialogProps) {
  const title =
    mode === 'single'
      ? 'Remove transcription?'
      : `Remove ${count} transcription${count === 1 ? '' : 's'}?`

  return (
    <div
      className="remove-dialog-backdrop"
      onClick={
        isRemoving
          ? undefined
          : (e) => {
              if (e.target === e.currentTarget) onCancel()
            }
      }
    >
      <div
        role="dialog"
        aria-modal="true"
        aria-labelledby="remove-dialog-title"
        className="remove-dialog"
        onClick={(e) => e.stopPropagation()}
      >
        <h3 id="remove-dialog-title">{title}</h3>
        {mode === 'single' && fileName && (
          <p className="remove-dialog__body">{fileName}</p>
        )}
        <p className="remove-dialog__warning">This cannot be undone.</p>
        {showProcessingNote && (
          <p className="remove-dialog__note">Processing will stop.</p>
        )}
        {error && <p className="remove-dialog__error">{error}</p>}
        <div className="remove-dialog__actions">
          <button type="button" onClick={onCancel} disabled={isRemoving}>
            Cancel
          </button>
          <button
            type="button"
            className="danger"
            onClick={onConfirm}
            disabled={isRemoving}
          >
            {isRemoving ? 'Removing…' : 'Remove'}
          </button>
        </div>
      </div>
    </div>
  )
}
