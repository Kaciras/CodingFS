namespace CodingFS.Workspaces
{
	public interface IWorkspaceFactory
	{
		IWorkspace? Match(string path);
	}
}
