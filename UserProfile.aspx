<%@ Page Title="" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="UserProfile.aspx.cs" Inherits="_180512B_Assignment01.UserProfile" %>

<asp:Content ID="Content2" ContentPlaceHolderID="HeadContent" runat="server">


</asp:Content>

<asp:Content ID="Content1" ContentPlaceHolderID="MainContent" runat="server">

    <div class="container" style="margin-top:40px" runat="server">

         <asp:Label ID="lb_smessage" class="alert alert-info" role="alert" runat="server"></asp:Label>
         <asp:ValidationSummary CssClass="alert alert-danger" ValidationGroup="customValidator" ID="ValidationSummary_login" runat="server" />

         <h1>User Profile</h1>
        <br />
        <div class="row">

            <div class="col form-group">
                <asp:Label ID="lb_firstName" runat="server" Text="First Name:"></asp:Label>
                <asp:TextBox ID="tb_firstName" class="form-control" runat="server"></asp:TextBox>
            </div>

            <div class="col form-group">
                <asp:Label ID="lb_lastName" runat="server" Text="Last Name:"></asp:Label>
                <asp:TextBox ID="tb_lastName" class="form-control" runat="server"></asp:TextBox>
            </div>

        </div>

        <div class="row">

            <div class="col form-group">
                <asp:Label ID="lb_dob" runat="server" Text="Date of Birth:"></asp:Label>
                <asp:TextBox ID="tb_dob" class="form-control" runat="server"></asp:TextBox>
            </div>

        </div>

        <div class="row">

            <div class="col form-group">
                <asp:Label ID="lb_creditCard" runat="server" Text="Credit Card Number:"></asp:Label>
                <asp:TextBox ID="tb_creditCard" class="form-control" runat="server"></asp:TextBox>
            </div>

        </div>

        <div class="row">
            <div class="col form-group">
                <asp:Label ID="lb_email" runat="server" Text="Email Address:"></asp:Label>
                <asp:TextBox ID="tb_email" class="form-control" runat="server"></asp:TextBox>
            </div>

        </div>
            
        <asp:Button ID="btn_logout" class="btn btn-primary" runat="server" Text="Logout" OnClick="btn_logout_Click"  />
          <asp:Button ID="btn_changePassword" class="btn btn-secondary" runat="server" Text="Change Password"  CausesValidation="False" OnClick="btn_changePassword_Click" />
        
    </div>


</asp:Content>
