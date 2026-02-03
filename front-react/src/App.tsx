import './App.css'
import { useQuery } from '@tanstack/react-query'
import { getConversations, type ConversationSummary } from './api-client'

function App() {
  const {
    data: conversations = [],
    isLoading,
    error,
  } = useQuery<ConversationSummary[], Error>({
    queryKey: ['conversations'],
    queryFn: getConversations,
    select: (data) =>
      [...data].sort(
        (a, b) =>
          new Date(b.created).getTime() - new Date(a.created).getTime(),
      ),
  })

  if (isLoading) return <div className="container"><p>Loading conversations...</p></div>
  if (error) return <div className="container"><p style={{ color: 'red' }}>Error: {error.message}</p></div>

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
