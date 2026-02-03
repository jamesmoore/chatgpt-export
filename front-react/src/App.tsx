import { useState, useEffect } from 'react'
import './App.css'
import { getConversations, type ConversationSummary } from './api-client'

function App() {
  const [conversations, setConversations] = useState<ConversationSummary[]>([])
  const [loading, setLoading] = useState(true)
  const [error, setError] = useState<string | null>(null)

  useEffect(() => {
    const fetchConversations = async () => {
      try {
        const data = await getConversations()
        // Sort by created date descending
        const sorted = data.sort((a, b) => 
          new Date(b.created).getTime() - new Date(a.created).getTime()
        )
        setConversations(sorted)
      } catch (err) {
        setError(err instanceof Error ? err.message : 'Failed to fetch conversations')
      } finally {
        setLoading(false)
      }
    }

    fetchConversations()
  }, [])

  if (loading) return <div className="container"><p>Loading conversations...</p></div>
  if (error) return <div className="container"><p style={{ color: 'red' }}>Error: {error}</p></div>

  return (
    <div className="container">
      <h1>Conversations</h1>
      {conversations.length === 0 ? (
        <p>No conversations found</p>
      ) : (
        <table>
          <thead>
            <tr>
              <th>Title</th>
              <th>Gizmo ID</th>
              <th>Created</th>
              <th>Updated</th>
            </tr>
          </thead>
          <tbody>
            {conversations.map((conv) => (
              <tr key={conv.id}>
                <td><a href={`/conversations/${conv.id}/html`}>{conv.title}</a></td>
                <td>{conv.gizmoId ?? '-'}</td>
                <td>{new Date(conv.created).toLocaleString()}</td>
                <td>{new Date(conv.updated).toLocaleString()}</td>
              </tr>
            ))}
          </tbody>
        </table>
      )}
    </div>
  )
}

export default App
