import './index.css'
import './custom.css'
import { StrictMode } from 'react'
import { createRoot } from 'react-dom/client'
import Layout from './layout.tsx'
import { ThemeProvider } from './components/theme-provider.tsx'
import { QueryClient, QueryClientProvider } from '@tanstack/react-query'
import { Route, BrowserRouter as Router, Routes } from 'react-router-dom'
import { ConversationPanel } from './conversation-panel.tsx'
import { TopBar } from './top-bar.tsx'

const queryClient = new QueryClient()

createRoot(document.getElementById('root')!).render(
  <StrictMode>
    <QueryClientProvider client={queryClient}>
      <ThemeProvider defaultTheme="system" storageKey="vite-ui-theme">
        <Router>
          <Routes>
            <Route path="/" element={<Layout />} />
            <Route path="/conversation/:id/:format" element={<Layout topBarChildren={<TopBar />}><ConversationPanel /></Layout>} />
          </Routes>
        </Router>
      </ThemeProvider>
    </QueryClientProvider>
  </StrictMode>,
)
