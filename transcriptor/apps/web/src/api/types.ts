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

export interface TranscriptSegment {
  index: number
  speakerKey: string
  startSeconds: number
  endSeconds: number
  text: string
}

export interface TranscriptionSpeaker {
  speakerKey: string
  defaultLabel: string
  displayName: string
}

export interface TranscriptionJobDetail extends TranscriptionJobListItem {
  transcriptText: string | null
  detectedLanguage: string | null
  hasDiarization: boolean
  segments: TranscriptSegment[]
  speakers: TranscriptionSpeaker[]
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
