import type { TranscriptionJobDetail } from '../../../api/types'
import { ExportToolbar } from './export-toolbar'
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
  if (isLoading && !job) {
    return <div className="job-detail job-detail--loading">Loading…</div>
  }

  if (!job) {
    return null
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
          <ExportToolbar fileName={job.fileName} transcriptText={job.transcriptText} />
          <pre className="job-detail__transcript">{job.transcriptText}</pre>
        </>
      )}
    </div>
  )
}
