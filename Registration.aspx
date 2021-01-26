<%@ Page Title="" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="Registration.aspx.cs" Inherits="_180512B_Assignment01.Registration" %>

<asp:Content ID="Content2" ContentPlaceHolderID="HeadContent" runat="server">


</asp:Content>
<asp:Content ID="Content1" ContentPlaceHolderID="MainContent" runat="server">

    <div class="container" style="margin-top: 40px">

        <asp:ValidationSummary CssClass="alert alert-danger" ValidationGroup="customValidator" ID="ValidationSummary" runat="server" />

        <h1>Registration</h1>
        <br />
        <div class="row">

            <div class="col form-group">
                <asp:Label ID="lb_firstName" runat="server" Text="First Name:"></asp:Label>
                <asp:TextBox ID="tb_firstName" class="form-control" runat="server"></asp:TextBox>
                <asp:RequiredFieldValidator ID="rfv_fristName" runat="server" ErrorMessage="First name cannot be empty" CssClass="text-danger" Display="Dynamic" ControlToValidate="tb_firstName"></asp:RequiredFieldValidator>
            </div>

            <div class="col form-group">
                <asp:Label ID="lb_lastName" runat="server" Text="Last Name:"></asp:Label>
                <asp:TextBox ID="tb_lastName" class="form-control" runat="server"></asp:TextBox>
                <asp:RequiredFieldValidator ID="rfv_lastName" runat="server" ErrorMessage="Last name cannot be empty" CssClass="text-danger" Display="Dynamic" ControlToValidate="tb_lastName"></asp:RequiredFieldValidator>
            </div>

        </div>

        <div class="row">

            <div class="col form-group">
                <asp:Label ID="lb_dob" runat="server" Text="Date of Birth:"></asp:Label>
                <asp:TextBox ID="tb_dob" class="form-control" runat="server" TextMode="Date"></asp:TextBox>
                <asp:RequiredFieldValidator ID="rfv_dob" runat="server" ErrorMessage="Date of Birth cannot be empty" CssClass="text-danger" Display="Dynamic" ControlToValidate="tb_dob"></asp:RequiredFieldValidator>
            </div>

        </div>

        <div class="row">

            <div class="col form-group">
                <asp:Label ID="lb_creditCard" runat="server" Text="Credit Card Number:"></asp:Label>
                <asp:TextBox ID="tb_creditCard" class="form-control" runat="server"></asp:TextBox>
                <asp:RequiredFieldValidator ID="rfv_creditCard" runat="server" ErrorMessage="Credit card number cannot be empty" CssClass="text-danger" Display="Dynamic" ControlToValidate="tb_creditCard"></asp:RequiredFieldValidator>
            </div>

        </div>

        <div class="row">
            <div class="col form-group">
                <asp:Label ID="lb_email" runat="server" Text="Email Address:"></asp:Label>
                <asp:TextBox ID="tb_email" class="form-control" runat="server"></asp:TextBox>
                <asp:RequiredFieldValidator ID="rfv_email" runat="server" ErrorMessage="Email cannot be empty" CssClass="text-danger" Display="Dynamic" ControlToValidate="tb_email"></asp:RequiredFieldValidator>
             </div>

        </div>

        <div class="row">
            <div class="col form-group">
                <asp:Label ID="lb_password" runat="server" Text="Password:"></asp:Label>
                <asp:TextBox ID="tb_password" class="form-control" runat="server" TextMode="Password"></asp:TextBox>
                 <asp:RequiredFieldValidator ID="rfv_password" runat="server" ErrorMessage="Password cannot be empty" CssClass="text-danger" Display="Dynamic" ControlToValidate="tb_password"></asp:RequiredFieldValidator>
                <asp:CustomValidator ID="cv_password" runat="server" ErrorMessage="CustomValidator" CssClass="text-danger" Display="Dynamic" ControlToValidate="tb_password" ClientValidationFunction="getPasswordRequirement" onservervalidate="cv_password_ServerValidate"></asp:CustomValidator>
            </div>
        </div>
        <asp:Button ID="btn_register" CssClass="btn btn-primary" runat="server" Text="Register" OnClick="btn_register_Click" />
        <asp:Button ID="btn_login" CssClass="btn btn-secondary" runat="server" Text="Login Instead" CausesValidation="False" OnClick="btn_login_Click" />
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
