<%@ Page Language="C#" Inherits="System.Web.Mvc.ViewPage<dynamic>" %>

<%@ Register Assembly="CADControl" Namespace="WebCAD" TagPrefix="cc1" %>

<div>
    <form id="Form1" runat="server">
        <asp:scriptmanager id="ScriptManager1" runat="server">
            </asp:scriptmanager>
        <cc1:CADControl ID="CADControl1" runat="server" Height="100%" Width="100%" Service="/draw" DefaultSHXFont="simplex.shx" SHXSearchPaths="SHX\" UseSHXFonts="True" SHXDefaultPath="SHX\" />
    </form>
</div>

<script type="text/javascript">
    CADControl1.cad.show("<%=ViewBag.DrawingID%>");
    //<%=ViewBag.DrawingFile%>

    var popupPrint = function () {
        var params = ['height=' + 360, 'width=' + 400].join(',');
        var win = window.open('<%=CADControl1.GetForm(CADControl.DialogForm.Print, "eng")%>', '_blank', params);
            $(win.document).ready(function () {
                win.cad = CADControl1.cad;
            });
        }
</script>