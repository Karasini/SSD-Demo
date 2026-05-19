import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query'
import {
  createTranscriptionJob,
  getTranscriptionJob,
  isActiveStatus,
  listTranscriptionJobs,
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
