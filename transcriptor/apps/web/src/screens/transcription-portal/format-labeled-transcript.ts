import type { TranscriptionJobDetail, TranscriptionSpeaker } from '../../api/types'

export const MAX_SPEAKER_DISPLAY_NAME_LENGTH = 64

export function formatSegmentTime(seconds: number, useCeil = false): string {
  const totalSeconds = Math.max(0, useCeil ? Math.ceil(seconds) : Math.floor(seconds))
  const hours = Math.floor(totalSeconds / 3600)
  const minutes = Math.floor((totalSeconds % 3600) / 60)
  const secs = totalSeconds % 60
  return [hours, minutes, secs]
    .map((value) => String(value).padStart(2, '0'))
    .join(':')
}

export function resolveSpeakerDisplayName(
  speakers: TranscriptionSpeaker[],
  speakerKey: string,
): string {
  return (
    speakers.find((speaker) => speaker.speakerKey === speakerKey)?.displayName ??
    speakerKey
  )
}

export function formatLabeledTranscript(job: TranscriptionJobDetail): string {
  if (!job.hasDiarization || job.segments.length === 0) {
    return job.transcriptText ?? ''
  }

  return job.segments
    .map((segment) => {
      const start = formatSegmentTime(segment.startSeconds, false)
      const end = formatSegmentTime(segment.endSeconds, true)
      const name = resolveSpeakerDisplayName(job.speakers, segment.speakerKey)
      return `[${start} – ${end}] ${name}: ${segment.text}`
    })
    .join('\n')
}

export function validateSpeakerDisplayName(value: string): string | null {
  const trimmed = value.trim()
  if (!trimmed) {
    return 'Name is required.'
  }
  if (trimmed.length > MAX_SPEAKER_DISPLAY_NAME_LENGTH) {
    return `Name must be at most ${MAX_SPEAKER_DISPLAY_NAME_LENGTH} characters.`
  }
  return null
}
