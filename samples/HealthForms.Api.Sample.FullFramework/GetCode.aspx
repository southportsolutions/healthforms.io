<%@ Page Title="Get Token" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="GetCode.aspx.cs" Inherits="HealthForms.Api.Sample.FullFramework.GetCode"  Async="true" %>

<asp:Content ID="BodyContent" ContentPlaceHolderID="MainContent" runat="server">
    <main aria-labelledby="title">
        <h1 class="display-4">Tenant Token</h1>
        <p>Learn more about authorizing a <a href="https://github.com/southportsolutions/healthforms.io">HealthForms.io tenant</a>.</p>
        <form method="post"
              enctype="application/x-www-form-urlencoded">
            <asp:Label ID="lblTenantId" runat="server" Text="Tenant ID"></asp:Label>
            <asp:TextBox ID="txtTenantId" runat="server"></asp:TextBox>
            <asp:Button ID="SubmitButton" runat="server" Text="Submit" OnClick="SubmitButton_Click" />  
        </form>
    </main>
</asp:Content>
