import { useParams, useNavigate } from "react-router-dom";
import { Button } from "./components/ui/button";
import { ButtonGroup } from "./components/ui/button-group";
import { SidebarTrigger } from "./components/ui/sidebar";

export function TopBar() {
      const { id, format } = useParams();
      const navigate = useNavigate();
      
  return (
    <div className="flex items-center">
      <SidebarTrigger />

      <ButtonGroup className="ml-auto">
        <Button variant={format === 'html' ? 'default' : 'outline'} onClick={() => navigate(`/conversation/${id}/html`)}>Html</Button>
        <Button variant={format === 'markdown' ? 'default' : 'outline'} onClick={() => navigate(`/conversation/${id}/markdown`)}>Markdown</Button>
        <Button variant={format === 'json' ? 'default' : 'outline'} onClick={() => navigate(`/conversation/${id}/json`)}>JSON</Button>
      </ButtonGroup>
    </div>
  );
}