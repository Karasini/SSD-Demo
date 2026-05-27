export type TranscriptionJobStatus =
  | 'Queued'
  | 'InProgress'
  | 'Completed'
  | 'Failed'

export interface TranscriptionJobListItem {
  id: string
  fileName: string
  fileSizeBytes: number
  contentType: string
  status: TranscriptionJobStatus
  createdAt: string
  updatedAt: string
  completedAt: string | null
  failureReason: string | null
}

export interface TranscriptionJobDetail extends TranscriptionJobListItem {
  transcriptText: string | null
  detectedLanguage: string | null
  segments: TranscriptSegment[] | null
  speakers: SpeakerInfo[] | null
}

export interface TranscriptionJobListResponse {
  items: TranscriptionJobListItem[]
  page: number
  pageSize: number
  totalCount: number
}

export interface BulkDeleteFailureItem {
  id: string
  reason: string
}

export interface BulkDeleteTranscriptionJobsResponse {
  deletedIds: string[]
  failed: BulkDeleteFailureItem[]
}

export interface TranscriptSegment {
  speakerId: string
  speakerLabel: string
  startSec: number
  endSec: number
  text: string
}

export interface SpeakerInfo {
  speakerId: string
  displayName: string
  defaultLabel: string
}
