import { useState } from 'react'
import { isAxiosError } from 'axios'
import {
  useCreateTranscriptionJob,
  useTranscriptionJobDetail,
  useTranscriptionJobList,
} from '../../hooks/use-transcription-jobs'
import { JobDetail } from './components/job-detail'
import { JobListItem } from './components/job-list-item'
import { UploadZone } from './components/upload-zone'
import './transcription-portal.css'

export function TranscriptionPortal() {
  const [selectedJobId, setSelectedJobId] = useState<string | null>(null)
  const [showNewUpload, setShowNewUpload] = useState(true)
  const [uploadProgress, setUploadProgress] = useState(0)
  const [uploadError, setUploadError] = useState<string | null>(null)

  const listQuery = useTranscriptionJobList()
  const detailQuery = useTranscriptionJobDetail(selectedJobId)
  const createMutation = useCreateTranscriptionJob()

  const handleUpload = async (file: File) => {
    setUploadError(null)
    setUploadProgress(0)
    try {
      const job = await createMutation.mutateAsync({
        file,
        onUploadProgress: setUploadProgress,
      })
      setSelectedJobId(job.id)
      setShowNewUpload(false)
    } catch (err) {
      if (isAxiosError(err)) {
        const data = err.response?.data as {
          detail?: string
          errors?: Record<string, string[]>
        }
        if (err.response?.status === 413) {
          setUploadError(data?.detail ?? 'Maximum file size is 2 GB.')
        } else if (data?.errors?.file?.[0]) {
          setUploadError(data.errors.file[0])
        } else {
          setUploadError('Upload failed. Check your connection and try again.')
        }
      } else {
        setUploadError('Upload failed. Check your connection and try again.')
      }
    }
  }

  const jobs = listQuery.data?.items ?? []

  return (
    <div className="portal">
      <header className="portal__header">
        <h1>Transcriptor</h1>
        <button
          type="button"
          className="primary"
          onClick={() => {
            setShowNewUpload(true)
            setSelectedJobId(null)
            setUploadError(null)
          }}
        >
          New transcription
        </button>
      </header>

      <div className="portal__body">
        <aside className="portal__list">
          <h2>Transcriptions</h2>
          {listQuery.isLoading && <p className="muted">Loading jobs…</p>}
          {!listQuery.isLoading && jobs.length === 0 && (
            <p className="muted">No transcriptions yet. Upload a file to get started.</p>
          )}
          <div className="portal__list-items">
            {jobs.map((job) => (
              <JobListItem
                key={job.id}
                job={job}
                selected={job.id === selectedJobId}
                onSelect={(id) => {
                  setSelectedJobId(id)
                  setShowNewUpload(false)
                }}
              />
            ))}
          </div>
        </aside>

        <main className="portal__workspace">
          {showNewUpload && !selectedJobId && (
            <div className="portal__welcome">
              <h2>Upload audio or video</h2>
              <p>Start a new transcription. You can run several jobs at the same time.</p>
              <UploadZone
                onStart={handleUpload}
                isUploading={createMutation.isPending}
                uploadProgress={uploadProgress}
                error={uploadError}
              />
            </div>
          )}

          {showNewUpload && selectedJobId && (
            <UploadZone
              onStart={handleUpload}
              isUploading={createMutation.isPending}
              uploadProgress={uploadProgress}
              error={uploadError}
            />
          )}

          {selectedJobId && !showNewUpload && (
            <JobDetail job={detailQuery.data} isLoading={detailQuery.isLoading} />
          )}

          {!selectedJobId && !showNewUpload && (
            <div className="portal__welcome">
              <h2>Select a transcription</h2>
              <p>Choose a job from the list, or start a new upload.</p>
            </div>
          )}
        </main>
      </div>
    </div>
  )
}
