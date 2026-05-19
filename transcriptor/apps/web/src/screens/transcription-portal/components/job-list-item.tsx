import type { TranscriptionJobListItem } from '../../../api/types'
import './job-list-item.css'

type JobListItemProps = {
  job: TranscriptionJobListItem
  selected: boolean
  onSelect: (id: string) => void
}

function formatDate(value: string) {
  return new Date(value).toLocaleString()
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

export function JobListItem({ job, selected, onSelect }: JobListItemProps) {
  return (
    <button
      type="button"
      className={`job-list-item ${selected ? 'selected' : ''}`}
      onClick={() => onSelect(job.id)}
      title={job.fileName}
    >
      <div className="job-list-item__name">{job.fileName}</div>
      <div className="job-list-item__meta">
        <span className={`status-badge status-${job.status.toLowerCase()}`}>
          {statusLabel(job.status)}
        </span>
        <span className="job-list-item__date">{formatDate(job.createdAt)}</span>
      </div>
    </button>
  )
}
