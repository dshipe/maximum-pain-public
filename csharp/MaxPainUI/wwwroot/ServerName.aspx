<%@ Page Language="c#" %>
<script runat="server">
protected void Page_Load(object sender, EventArgs e)
{
	Response.Write("<h1>"+System.Environment.MachineName+"</h1>");
}
</script>