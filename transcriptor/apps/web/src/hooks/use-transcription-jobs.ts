import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query'
import {
  bulkDeleteTranscriptionJobs,
  createTranscriptionJob,
  deleteTranscriptionJob,
  getTranscriptionJob,
  isActiveStatus,
  listTranscriptionJobs,
  updateTranscriptionSpeakerLabel,
} from '../api/transcription-jobs'

const POLL_INTERVAL_MS = 3000

export function useTranscriptionJobList() {
  return useQuery({
    queryKey: ['transcription-jobs', 'list'],
    queryFn: () => listTranscriptionJobs(),
    refetchInterval: (query) => {
      const items = query.state.data?.items ?? []
      const hasActive = items.some((j) => isActiveStatus(j.status))
      return hasActive ? POLL_INTERVAL_MS : false
    },
  })
}

export function useTranscriptionJobDetail(jobId: string | null) {
  return useQuery({
    queryKey: ['transcription-jobs', 'detail', jobId],
    queryFn: () => getTranscriptionJob(jobId!),
    enabled: Boolean(jobId),
    refetchInterval: (query) => {
      const status = query.state.data?.status
      return status && isActiveStatus(status) ? POLL_INTERVAL_MS : false
    },
  })
}

export function useCreateTranscriptionJob() {
  const queryClient = useQueryClient()
  return useMutation({
    mutationFn: ({
      file,
      onUploadProgress,
    }: {
      file: File
      onUploadProgress?: (percent: number) => void
    }) => createTranscriptionJob(file, onUploadProgress),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['transcription-jobs'] })
    },
  })
}

export function useDeleteTranscriptionJob() {
  const queryClient = useQueryClient()
  return useMutation({
    mutationFn: (id: string) => deleteTranscriptionJob(id),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['transcription-jobs'] })
    },
  })
}

export function useBulkDeleteTranscriptionJobs() {
  const queryClient = useQueryClient()
  return useMutation({
    mutationFn: (ids: string[]) => bulkDeleteTranscriptionJobs(ids),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['transcription-jobs'] })
    },
  })
}

export function useUpdateSpeakerLabel(jobId: string | null) {
  const queryClient = useQueryClient()
  return useMutation({
    mutationFn: ({
      speakerKey,
      displayName,
    }: {
      speakerKey: string
      displayName: string
    }) => updateTranscriptionSpeakerLabel(jobId!, speakerKey, displayName),
    onSuccess: (detail) => {
      queryClient.setQueryData(['transcription-jobs', 'detail', detail.id], detail)
      queryClient.invalidateQueries({ queryKey: ['transcription-jobs', 'list'] })
    },
  })
}
