import { apiClient } from './client'
import type {
  TranscriptionJobDetail,
  TranscriptionJobListResponse,
} from './types'

export async function listTranscriptionJobs(
  page = 1,
  pageSize = 50,
): Promise<TranscriptionJobListResponse> {
  const { data } = await apiClient.get<TranscriptionJobListResponse>(
    '/api/v1/transcription-jobs',
    { params: { page, pageSize } },
  )
  return data
}

export async function getTranscriptionJob(
  id: string,
): Promise<TranscriptionJobDetail> {
  const { data } = await apiClient.get<TranscriptionJobDetail>(
    `/api/v1/transcription-jobs/${id}`,
  )
  return data
}

export async function createTranscriptionJob(
  file: File,
  onUploadProgress?: (percent: number) => void,
): Promise<TranscriptionJobDetail> {
  const formData = new FormData()
  formData.append('file', file)
  const { data } = await apiClient.post<TranscriptionJobDetail>(
    '/api/v1/transcription-jobs',
    formData,
    {
      headers: { 'Content-Type': 'multipart/form-data' },
      onUploadProgress: (event) => {
        if (!event.total || !onUploadProgress) return
        onUploadProgress(Math.round((event.loaded * 100) / event.total))
      },
    },
  )
  return data
}

export function isActiveStatus(status: string): boolean {
  return status === 'Queued' || status === 'InProgress'
}
