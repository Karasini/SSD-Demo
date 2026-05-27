import type { TranscriptionJobDetail } from '../../../api/types'
import {
  formatSegmentTime,
  resolveSpeakerDisplayName,
} from '../format-labeled-transcript'
import './labeled-transcript.css'

const SPEAKER_COLORS = [
  '#2563eb',
  '#059669',
  '#d97706',
  '#7c3aed',
  '#db2777',
  '#0891b2',
]

type LabeledTranscriptProps = {
  job: TranscriptionJobDetail
  onRenameSpeaker: (speakerKey: string, currentName: string) => void
}

function speakerColor(speakerKey: string, speakers: TranscriptionJobDetail['speakers']) {
  const index = speakers.findIndex((speaker) => speaker.speakerKey === speakerKey)
  return SPEAKER_COLORS[(index >= 0 ? index : 0) % SPEAKER_COLORS.length]
}

export function LabeledTranscript({ job, onRenameSpeaker }: LabeledTranscriptProps) {
  return (
    <div className="labeled-transcript">
      {job.segments.map((segment) => {
        const displayName = resolveSpeakerDisplayName(job.speakers, segment.speakerKey)
        const start = formatSegmentTime(segment.startSeconds, false)
        const end = formatSegmentTime(segment.endSeconds, true)
        const color = speakerColor(segment.speakerKey, job.speakers)

        return (
          <article key={segment.index} className="labeled-transcript__row">
            <div className="labeled-transcript__meta">
              <button
                type="button"
                className="labeled-transcript__speaker"
                style={{ color }}
                onClick={() => onRenameSpeaker(segment.speakerKey, displayName)}
                title="Rename speaker"
              >
                {displayName}
              </button>
              <span className="labeled-transcript__time">
                {start} – {end}
              </span>
            </div>
            <p className="labeled-transcript__text">{segment.text}</p>
          </article>
        )
      })}
    </div>
  )
}
