<%@ Page Title="Get Token" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="GetTenantToken.aspx.cs" Inherits="HealthForms.Api.Sample.FullFramework.GetTenantToken" %>

<asp:Content ID="BodyContent" ContentPlaceHolderID="MainContent" runat="server">
    <main aria-labelledby="title">
        <h2 id="title"><%: Title %>.</h2>
        <form method="post">
            <asp:Label ID="lblTenantId" runat="server" Text="Tenant ID"></asp:Label>
            <asp:TextBox ID="txtTenantId" runat="server"></asp:TextBox>
            <input class="btn btn-primary" type="submit" name="submit" value="Submit" />
        </form>
    </main>
</asp:Content>
