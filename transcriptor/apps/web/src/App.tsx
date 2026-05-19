import { QueryClient, QueryClientProvider } from '@tanstack/react-query'
import { TranscriptionPortal } from './screens/transcription-portal/transcription-portal'
import './index.css'

const queryClient = new QueryClient()

function App() {
  return (
    <QueryClientProvider client={queryClient}>
      <TranscriptionPortal />
    </QueryClientProvider>
  )
}

export default App
