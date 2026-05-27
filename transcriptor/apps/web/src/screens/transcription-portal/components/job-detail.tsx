import { useState } from 'react'
import type { TranscriptionJobDetail } from '../../../api/types'
import { useUpdateSpeakerLabel } from '../../../hooks/use-transcription-jobs'
import { formatLabeledTranscript } from '../format-labeled-transcript'
import { ExportToolbar } from './export-toolbar'
import { LabeledTranscript } from './labeled-transcript'
import { RenameSpeakerDialog } from './rename-speaker-dialog'
import './job-detail.css'

type JobDetailProps = {
  job: TranscriptionJobDetail | undefined
  isLoading: boolean
  onRemove: () => void
}

function statusLabel(status: string) {
  switch (status) {
    case 'Queued':
      return 'Queued'
    case 'InProgress':
      return 'In progress'
    case 'Completed':
      return 'Completed'
    case 'Failed':
      return 'Failed'
    default:
      return status
  }
}

export function JobDetail({ job, isLoading, onRemove }: JobDetailProps) {
  const updateSpeaker = useUpdateSpeakerLabel(job?.id ?? null)
  const [renameTarget, setRenameTarget] = useState<{
    speakerKey: string
    currentName: string
  } | null>(null)
  const [renameError, setRenameError] = useState<string | null>(null)

  if (isLoading && !job) {
    return (
      <div className="job-detail job-detail--loading">
        <div className="labeled-transcript-skeleton" aria-hidden>
          <div className="labeled-transcript-skeleton__line" style={{ width: '40%' }} />
          <div className="labeled-transcript-skeleton__line" style={{ width: '90%' }} />
          <div className="labeled-transcript-skeleton__line" style={{ width: '75%' }} />
        </div>
      </div>
    )
  }

  if (!job) {
    return null
  }

  const exportText =
    job.status === 'Completed'
      ? job.hasDiarization
        ? formatLabeledTranscript(job)
        : (job.transcriptText ?? '')
      : ''

  const handleRenameSave = async (displayName: string) => {
    if (!renameTarget) return
    setRenameError(null)
    try {
      await updateSpeaker.mutateAsync({
        speakerKey: renameTarget.speakerKey,
        displayName,
      })
      setRenameTarget(null)
    } catch {
      setRenameError('Could not save the new name. Please try again.')
    }
  }

  return (
    <div className="job-detail">
      <header className="job-detail__header">
        <div className="job-detail__title-row">
          <h2 title={job.fileName}>{job.fileName}</h2>
          <span className={`status-badge status-${job.status.toLowerCase()}`}>
            {statusLabel(job.status)}
          </span>
        </div>
        <button type="button" className="danger-text" onClick={onRemove}>
          Remove
        </button>
      </header>

      {(job.status === 'Queued' || job.status === 'InProgress') && (
        <div className="job-detail__processing">
          <div className="spinner" aria-hidden />
          <p>Transcription in progress…</p>
        </div>
      )}

      {job.status === 'Failed' && (
        <div className="job-detail__error">
          <p>{job.failureReason ?? 'Transcription failed. Please try again.'}</p>
          <p>Upload a new file to try again.</p>
        </div>
      )}

      {job.status === 'Completed' && (
        <>
          {exportText && (
            <ExportToolbar fileName={job.fileName} exportText={exportText} />
          )}

          {job.hasDiarization && job.segments.length > 0 && (
            <LabeledTranscript
              job={job}
              onRenameSpeaker={(speakerKey, currentName) => {
                setRenameError(null)
                setRenameTarget({ speakerKey, currentName })
              }}
            />
          )}

          {!job.hasDiarization && job.transcriptText && (
            <>
              <div className="job-detail__legacy-banner" role="status">
                Speaker labels are not available for this older transcription. Plain text is
                shown below.
              </div>
              <pre className="job-detail__transcript">{job.transcriptText}</pre>
            </>
          )}
        </>
      )}

      {renameTarget && (
        <RenameSpeakerDialog
          speakerKey={renameTarget.speakerKey}
          currentName={renameTarget.currentName}
          isSaving={updateSpeaker.isPending}
          errorMessage={renameError}
          onCancel={() => {
            setRenameTarget(null)
            setRenameError(null)
          }}
          onSave={handleRenameSave}
        />
      )}
    </div>
  )
}
