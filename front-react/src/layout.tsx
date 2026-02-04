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
      <AppSidebar />

      <div className="flex flex-col w-full min-w-0">
        {topBarChildren &&
          <div className="sticky top-0">
            <div className="bg-sidebar p-1 pr-3">
              {topBarChildren}
            </div>
            <div className="shrink-0 h-px w-full bg-sidebar-border"></div>
          </div>
        }
        <div className="flex w-full flex-col " >
          {children}
        </div>
      </div>
    </SidebarProvider>
  )
}

