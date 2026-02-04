import type { ReactNode } from 'react';
import { AppSidebar } from './components/app-sidebar'
import { SidebarProvider } from './components/ui/sidebar'

export interface LayoutProps {
  children?: ReactNode;
  topBarChildren?: ReactNode;
}

export default function Layout({ children, topBarChildren }: LayoutProps) {

  return (
    <SidebarProvider >
      <div className="flex flex-1 h-screen overflow-hidden">
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
  )
}

