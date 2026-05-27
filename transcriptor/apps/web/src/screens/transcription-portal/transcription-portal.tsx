import { useMemo, useState } from 'react'
import { isAxiosError } from 'axios'
import { isActiveStatus } from '../../api/transcription-jobs'
import {
  useBulkDeleteTranscriptionJobs,
  useCreateTranscriptionJob,
  useDeleteTranscriptionJob,
  useRenameSpeaker,
  useTranscriptionJobDetail,
  useTranscriptionJobList,
} from '../../hooks/use-transcription-jobs'
import { JobDetail } from './components/job-detail'
import { JobListItem } from './components/job-list-item'
import { RemoveConfirmDialog } from './components/remove-confirm-dialog'
import { UploadZone } from './components/upload-zone'
import './transcription-portal.css'

type PendingRemove =
  | { mode: 'single'; jobId: string; fileName: string; showProcessingNote: boolean }
  | { mode: 'bulk'; ids: string[]; showProcessingNote: boolean }

export function TranscriptionPortal() {
  const [selectedJobId, setSelectedJobId] = useState<string | null>(null)
  const [showNewUpload, setShowNewUpload] = useState(true)
  const [uploadProgress, setUploadProgress] = useState(0)
  const [uploadError, setUploadError] = useState<string | null>(null)
  const [selectionMode, setSelectionMode] = useState(false)
  const [checkedIds, setCheckedIds] = useState<Set<string>>(() => new Set())
  const [pendingRemove, setPendingRemove] = useState<PendingRemove | null>(null)
  const [removeError, setRemoveError] = useState<string | null>(null)
  const [bulkPartialMessage, setBulkPartialMessage] = useState<string | null>(null)

  const listQuery = useTranscriptionJobList()
  const detailQuery = useTranscriptionJobDetail(selectedJobId)
  const createMutation = useCreateTranscriptionJob()
  const deleteMutation = useDeleteTranscriptionJob()
  const bulkDeleteMutation = useBulkDeleteTranscriptionJobs()
  const renameSpeakerMutation = useRenameSpeaker()

  const jobs = listQuery.data?.items ?? []
  const isRemoving = deleteMutation.isPending || bulkDeleteMutation.isPending

  const checkedCount = checkedIds.size

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

  const exitSelectionMode = () => {
    setSelectionMode(false)
    setCheckedIds(new Set())
    setBulkPartialMessage(null)
  }

  const toggleChecked = (id: string) => {
    setCheckedIds((prev) => {
      const next = new Set(prev)
      if (next.has(id)) next.delete(id)
      else next.add(id)
      return next
    })
  }

  const clearSelectionIfRemoved = (removedIds: string[]) => {
    if (selectedJobId && removedIds.includes(selectedJobId)) {
      setSelectedJobId(null)
      setShowNewUpload(false)
    }
  }

  const openSingleRemove = (jobId: string, fileName: string, status: string) => {
    setRemoveError(null)
    setPendingRemove({
      mode: 'single',
      jobId,
      fileName,
      showProcessingNote: isActiveStatus(status),
    })
  }

  const openBulkRemove = () => {
    const ids = Array.from(checkedIds)
    if (ids.length === 0) return
    const selectedJobs = jobs.filter((j) => ids.includes(j.id))
    const showProcessingNote = selectedJobs.some((j) => isActiveStatus(j.status))
    setRemoveError(null)
    setPendingRemove({ mode: 'bulk', ids, showProcessingNote })
  }

  const closeRemoveDialog = () => {
    if (isRemoving) return
    setPendingRemove(null)
    setRemoveError(null)
  }

  const handleConfirmRemove = async () => {
    if (!pendingRemove) return
    setRemoveError(null)
    setBulkPartialMessage(null)

    try {
      if (pendingRemove.mode === 'single') {
        try {
          await deleteMutation.mutateAsync(pendingRemove.jobId)
        } catch (err) {
          if (isAxiosError(err) && err.response?.status === 404) {
            clearSelectionIfRemoved([pendingRemove.jobId])
            setPendingRemove(null)
            return
          }
          throw err
        }
        clearSelectionIfRemoved([pendingRemove.jobId])
        setPendingRemove(null)
        return
      }

      const result = await bulkDeleteMutation.mutateAsync(pendingRemove.ids)
      clearSelectionIfRemoved(result.deletedIds)

      if (result.failed.length > 0) {
        setBulkPartialMessage('Some transcriptions could not be removed.')
        setCheckedIds(new Set(result.failed.map((f) => f.id)))
        setSelectionMode(true)
      } else {
        exitSelectionMode()
      }

      setPendingRemove(null)
    } catch {
      setRemoveError('Could not remove. Check your connection and try again.')
    }
  }

  const dialogProps = useMemo(() => {
    if (!pendingRemove) return null
    if (pendingRemove.mode === 'single') {
      return {
        mode: 'single' as const,
        fileName: pendingRemove.fileName,
        count: 1,
        showProcessingNote: pendingRemove.showProcessingNote,
      }
    }
    return {
      mode: 'bulk' as const,
      fileName: undefined,
      count: pendingRemove.ids.length,
      showProcessingNote: pendingRemove.showProcessingNote,
    }
  }, [pendingRemove])

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
          <div className="portal__list-header">
            <h2>Transcriptions</h2>
            <div className="portal__list-actions">
              {!selectionMode ? (
                <button
                  type="button"
                  className="secondary"
                  onClick={() => setSelectionMode(true)}
                  disabled={jobs.length === 0}
                >
                  Select
                </button>
              ) : (
                <>
                  <button type="button" onClick={exitSelectionMode}>
                    Cancel
                  </button>
                  <span className="portal__selection-count">{checkedCount} selected</span>
                  <button
                    type="button"
                    className="danger"
                    disabled={checkedCount === 0 || isRemoving}
                    onClick={openBulkRemove}
                  >
                    {isRemoving ? 'Removing…' : 'Remove selected'}
                  </button>
                </>
              )}
            </div>
          </div>
          {bulkPartialMessage && (
            <p className="portal__bulk-message">{bulkPartialMessage}</p>
          )}
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
                selectionMode={selectionMode}
                checked={checkedIds.has(job.id)}
                onSelect={(id) => {
                  setSelectedJobId(id)
                  setShowNewUpload(false)
                }}
                onToggleCheck={toggleChecked}
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
            <JobDetail
              job={detailQuery.data}
              isLoading={detailQuery.isLoading}
              isRenamingSpeaker={renameSpeakerMutation.isPending}
              onRenameSpeaker={async (speakerId, displayName) => {
                if (!selectedJobId) return
                await renameSpeakerMutation.mutateAsync({
                  jobId: selectedJobId,
                  speakerId,
                  displayName,
                })
              }}
              onRemove={() => {
                const job = detailQuery.data ?? jobs.find((j) => j.id === selectedJobId)
                if (!job) return
                openSingleRemove(job.id, job.fileName, job.status)
              }}
            />
          )}

          {!selectedJobId && !showNewUpload && (
            <div className="portal__welcome">
              <h2>Select a transcription</h2>
              <p>Choose a job from the list, or start a new upload.</p>
            </div>
          )}
        </main>
      </div>

      {dialogProps && (
        <RemoveConfirmDialog
          mode={dialogProps.mode}
          fileName={dialogProps.fileName}
          count={dialogProps.count}
          showProcessingNote={dialogProps.showProcessingNote}
          isRemoving={isRemoving}
          error={removeError}
          onCancel={closeRemoveDialog}
          onConfirm={handleConfirmRemove}
        />
      )}
    </div>
  )
}
