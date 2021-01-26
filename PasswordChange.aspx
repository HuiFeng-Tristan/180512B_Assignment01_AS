<%@ Page Title="" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="PasswordChange.aspx.cs" Inherits="_180512B_Assignment01.PasswordChange" %>

<asp:Content ID="Content1" ContentPlaceHolderID="HeadContent" runat="server">
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="MainContent" runat="server">

    <div class="container" style="margin-top: 40px">

          <asp:Label ID="lb_smessage" class="alert alert-info" role="alert" runat="server"></asp:Label>
        <asp:ValidationSummary CssClass="alert alert-danger" ValidationGroup="customValidator" ID="ValidationSummary_pc" runat="server" />

        <h1>Change Password</h1>
        <br />

        <div class="form-group">
            <asp:Label ID="lb_password" runat="server" Text="Password:"></asp:Label>
            <asp:TextBox ID="tb_password" class="form-control" runat="server" TextMode="Password"></asp:TextBox>
            <asp:RequiredFieldValidator ID="rfv_password" runat="server" ErrorMessage="Password cannot be empty" CssClass="text-danger" Display="Dynamic" ControlToValidate="tb_password"></asp:RequiredFieldValidator>
            <asp:CustomValidator ID="cv_password" runat="server" ErrorMessage="CustomValidator" CssClass="text-danger" Display="Dynamic" ControlToValidate="tb_password" ClientValidationFunction="getPasswordRequirement" OnServerValidate="cv_password_ServerValidate"></asp:CustomValidator>
        </div>

        <div class="form-group">
            <asp:Label ID="lb_cPassword" runat="server" Text="Confirm Password:"></asp:Label>
            <asp:TextBox ID="tb_cPassword" class="form-control" runat="server" TextMode="Password"></asp:TextBox>
            <asp:RequiredFieldValidator ID="rfv_cpass" runat="server" ErrorMessage="This field cannot be empty" CssClass="text-danger" Display="Dynamic" ControlToValidate="tb_password"></asp:RequiredFieldValidator>
        </div>

        <asp:Button ID="btn_changePassword" class="btn btn-primary" runat="server" Text="Change Password" OnClick="btn_changePassword_Click" />
    </div>

    <script>

        function getPasswordRequirement(sender, args) {

            console.log("length");
            console.log(args.Value.length);
            console.log(args)
            console.log(args.Value.search(/^\S*$/) == -1)

            if (args.Value.search(/^\S*$/) == -1) {
                sender.innerHTML = "Password cannot contain space";
                args.IsValid = false;

            }
            else if (args.Value.length < 8) {
                sender.innerHTML = "Password must be at least 8 characters";
                args.IsValid = false;

            }

            else if (args.Value.search(/[0-9]/) == -1) {
                sender.innerHTML = "Password have be at least 1 number";
                args.IsValid = false;

            }
            else if (args.Value.search(/[A-Z]/) == -1) {
                sender.innerHTML = "Password have at least 1 uppercase";
                args.IsValid = false;

            }
            else if (args.Value.search(/[a-z]/) == -1) {
                sender.innerHTML = "Password have at least 1 lowercase";
                args.IsValid = false;

            }
            else if (args.Value.search(/[^A-Za-z0-9]/) == -1) {
                sender.innerHTML = "Password have at least 1 special character";
                args.IsValid = false;

            }

            else {
                sender.innerHTML = "";
                args.IsValid = true;

            }

        }

    </script>
</asp:Content>
