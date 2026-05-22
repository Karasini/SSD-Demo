import type { TranscriptionJobListItem } from '../../../api/types'
import './job-list-item.css'

type JobListItemProps = {
  job: TranscriptionJobListItem
  selected: boolean
  selectionMode: boolean
  checked: boolean
  onSelect: (id: string) => void
  onToggleCheck: (id: string) => void
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

export function JobListItem({
  job,
  selected,
  selectionMode,
  checked,
  onSelect,
  onToggleCheck,
}: JobListItemProps) {
  const rowClass = [
    'job-list-item',
    selected && !selectionMode ? 'selected' : '',
    selectionMode && checked ? 'checked' : '',
  ]
    .filter(Boolean)
    .join(' ')

  const handleRowClick = () => {
    if (selectionMode) {
      onToggleCheck(job.id)
      return
    }
    onSelect(job.id)
  }

  return (
    <div className={rowClass}>
      {selectionMode && (
        <input
          type="checkbox"
          className="job-list-item__checkbox"
          checked={checked}
          onChange={() => onToggleCheck(job.id)}
          onClick={(e) => e.stopPropagation()}
          aria-label={`Select ${job.fileName}`}
        />
      )}
      <button
        type="button"
        className="job-list-item__button"
        onClick={handleRowClick}
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
    </div>
  )
}
