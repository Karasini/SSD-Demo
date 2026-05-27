import { useState } from 'react'
import type { SpeakerInfo, TranscriptionJobDetail, TranscriptSegment } from '../../../api/types'
import { ExportToolbar } from './export-toolbar'
import './job-detail.css'

type JobDetailProps = {
  job: TranscriptionJobDetail | undefined
  isLoading: boolean
  isRenamingSpeaker: boolean
  onRenameSpeaker: (speakerId: string, displayName: string) => Promise<void>
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

function formatTime(value: number): string {
  const total = Math.max(0, value)
  const minutes = Math.floor(total / 60)
  const seconds = total - minutes * 60
  return `${String(minutes).padStart(2, '0')}:${seconds.toFixed(2).padStart(5, '0')}`
}

function formatSegmentsForExport(segments: TranscriptSegment[]): string {
  return segments
    .map(
      (segment) =>
        `${segment.speakerLabel} [${formatTime(segment.startSec)}-${formatTime(segment.endSec)}]: ${segment.text}`,
    )
    .join('\n')
}

export function JobDetail({
  job,
  isLoading,
  isRenamingSpeaker,
  onRenameSpeaker,
  onRemove,
}: JobDetailProps) {
  const [renameValues, setRenameValues] = useState<Record<string, string>>({})
  const [renameError, setRenameError] = useState<string | null>(null)
  const [renameSuccess, setRenameSuccess] = useState<string | null>(null)

  if (isLoading && !job) {
    return <div className="job-detail job-detail--loading">Loading…</div>
  }

  if (!job) {
    return null
  }

  const speakers = job.speakers ?? []
  const segments = job.segments ?? []
  const exportText =
    segments.length === 0 ? (job.transcriptText ?? '') : formatSegmentsForExport(segments)

  const updateRenameValue = (speaker: SpeakerInfo, value: string) => {
    setRenameValues((prev) => ({ ...prev, [speaker.speakerId]: value }))
  }

  const getCurrentRenameValue = (speaker: SpeakerInfo) =>
    renameValues[speaker.speakerId] ?? speaker.displayName

  const handleRename = async (speaker: SpeakerInfo) => {
    const value = getCurrentRenameValue(speaker)
    setRenameError(null)
    setRenameSuccess(null)
    try {
      await onRenameSpeaker(speaker.speakerId, value)
      setRenameSuccess(`Saved speaker name: ${value.trim()}`)
    } catch {
      setRenameError('Could not save speaker name. Try again.')
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

      {job.status === 'Completed' && job.transcriptText && (
        <>
          <ExportToolbar fileName={job.fileName} transcriptText={exportText} />

          {speakers.length > 0 && (
            <section className="job-detail__speakers">
              <h3>Speakers</h3>
              <div className="job-detail__speaker-list">
                {speakers.map((speaker) => (
                  <div key={speaker.speakerId} className="job-detail__speaker-item">
                    <span className="job-detail__speaker-default">{speaker.defaultLabel}</span>
                    <input
                      type="text"
                      value={getCurrentRenameValue(speaker)}
                      onChange={(event) => updateRenameValue(speaker, event.target.value)}
                      maxLength={80}
                    />
                    <button
                      type="button"
                      onClick={() => handleRename(speaker)}
                      disabled={isRenamingSpeaker}
                    >
                      {isRenamingSpeaker ? 'Saving…' : 'Save'}
                    </button>
                  </div>
                ))}
              </div>
              {renameError && <p className="job-detail__rename-error">{renameError}</p>}
              {renameSuccess && <p className="job-detail__rename-success">{renameSuccess}</p>}
            </section>
          )}

          {segments.length > 0 ? (
            <div className="job-detail__segments">
              {segments.map((segment, index) => (
                <div key={`${segment.speakerId}-${index}`} className="job-detail__segment">
                  <div className="job-detail__segment-meta">
                    <strong>{segment.speakerLabel}</strong>
                    <span>
                      {formatTime(segment.startSec)} - {formatTime(segment.endSec)}
                    </span>
                  </div>
                  <p>{segment.text}</p>
                </div>
              ))}
            </div>
          ) : (
            <pre className="job-detail__transcript">{job.transcriptText}</pre>
          )}
        </>
      )}
    </div>
  )
}
