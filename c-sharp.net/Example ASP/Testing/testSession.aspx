<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="testSession.aspx.cs" Inherits="Testing.testSession" %>

<!DOCTYPE html>

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title></title>
    <style type="text/css">
        .auto-style2 {
            width: 205px;
        }
        .auto-style3 {
            height: 131px;
            width: 23px;
        }
        .auto-style4 {
            width: 205px;
            height: 131px;
        }
        .auto-style5 {
            height: 106px;
            width: 23px;
        }
        .auto-style6 {
            width: 205px;
            height: 106px;
        }
        .auto-style7 {
            width: 23px;
        }
        .auto-style8 {
            width: 80%;
            height: 266px;
        }
    </style>
</head>
<body style="width: 542px; height: 69px">
    <form id="form1" runat="server">
        <div>
            <table class="auto-style8">
                <tr>
                    <td class="auto-style5">
                        <asp:Button ID="Button1" runat="server" OnClick="Button1_Click" Text="Button" />
                    </td>
                    <td class="auto-style6">
                        <asp:Label ID="txtResponse" runat="server" Text="Response"></asp:Label>
                    </td>
                </tr>
                <tr>
                    <td class="auto-style7">&nbsp;</td>
                    <td class="auto-style2">&nbsp;</td>
                </tr>
            </table>
        </div>
    </form>
</body>
</html>
