import type { ReactNode } from 'react';
import { AppSidebar } from './components/app-sidebar'
import { SidebarProvider } from './components/ui/sidebar'
import { DocumentTitle } from './document-title';
import { useSystemThemeListener } from './hooks/use-system-theme-listener';
import { useConversations } from './hooks/use-conversations';
import LoadingSpinner from './loading-spinner';

export interface LayoutProps {
  children?: ReactNode;
  topBarChildren?: ReactNode;
}

export default function Layout({ children, topBarChildren }: LayoutProps) {
  useSystemThemeListener();

  const {
    isLoading,
    error,
  } = useConversations();

  return (
    <>
      <DocumentTitle />
      {isLoading &&
        <div className="flex h-screen">
          <LoadingSpinner />
        </div>
      }

      {error && !isLoading &&
        <div className="flex h-screen items-center justify-center text-red-600">
          Error: {error.message}
        </div>
      }

      {!isLoading && !error &&
        <SidebarProvider >
          <div className="flex flex-1 h-screen overflow-hidden bg-neutral-100 dark:bg-neutral-900">
            <AppSidebar />
            <div className="flex-1 min-w-0 overflow-auto flex flex-col">
              {topBarChildren &&
                <div className="sticky top-0">
                  <div className="bg-sidebar p-1 pr-3">
                    {topBarChildren}
                  </div>
                  <div className="shrink-0 h-px w-full bg-sidebar-border"></div>
                </div>
              }
              {children}
            </div>
          </div>
        </SidebarProvider >

      }
    </>
  )
}

