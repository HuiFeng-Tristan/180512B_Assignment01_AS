<%@ Page Title="" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="LoginMain.aspx.cs" Inherits="_180512B_Assignment01.LoginMain" %>

<asp:Content ID="Content2" ContentPlaceHolderID="HeadContent" runat="server">

    <script src="https://www.google.com/recaptcha/api.js?render=6LcCBTsaAAAAAAvuAyvvPi-KBV6JLrYeC5Ebhrz2"></script>

</asp:Content>

<asp:Content ID="Content1" ContentPlaceHolderID="MainContent" runat="server">


    <div class="container" style="margin-top: 40px">
        
        <asp:Label ID="lb_smessage" class="alert alert-info" role="alert" runat="server"></asp:Label>
        <asp:ValidationSummary CssClass="alert alert-danger" ValidationGroup="customValidator" ID="ValidationSummary_login" runat="server" />

        <h1>Login</h1>
        <br />
        <div class="form-group">

            <asp:Label ID="lb_loginID" runat="server" Text="Username:"></asp:Label>
            <asp:TextBox ID="tb_loginID" class="form-control" runat="server"></asp:TextBox>
        </div>
        <div class="form-group">
            <asp:Label ID="lb_password" runat="server" Text="Password:"></asp:Label>
            <asp:TextBox ID="tb_password" class="form-control" runat="server" TextMode="Password"></asp:TextBox>
        </div>

        <asp:Button ID="btn_login" class="btn btn-primary" runat="server" Text="Login" OnClick="btn_login_Click" />
        <asp:Button ID="btn_register" class="btn btn-secondary" runat="server" Text="Create a new account" CausesValidation="False" OnClick="btn_register_Click" />
    </div>

    <input type="hidden" id="g-recaptcha-response" name="g-recaptcha-response" />
    <!--<asp:Label ID="lb_score" runat="server" Text="Score:"></asp:Label>-->

    <script>
        grecaptcha.ready(function () {
            grecaptcha.execute('6LcCBTsaAAAAAAvuAyvvPi-KBV6JLrYeC5Ebhrz2', { action: 'Login' }).then(function (token) {
                document.getElementById("g-recaptcha-response").value = token;
            });
        });
    </script>

</asp:Content>
