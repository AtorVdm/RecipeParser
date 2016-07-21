<%@ Page Title="Home Page" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="Default.aspx.cs" Inherits="RecipeParser._Default" %>

<asp:Content ID="BodyContent" ContentPlaceHolderID="MainContent" runat="server">
    <div class="jumbotron pageContent">
        <asp:Label ID="errorLabel" style="color:red" Text="" runat="server"/>
        <p />
        <div class="fileUpload btn btn-primary">
            <div id="uploadBtnText">Upload</div>
            <asp:FileUpload runat="server" ID="uploadedFile" AllowMultiple="true" type="file" class="upload" />
        </div>
        &nbsp;
        <asp:Button runat="server" ID="btnSubmit" OnClick="btnSubmitClick" Text="Submit"  CssClass="btn btn-danger submitButton" />
        <p />
        <p id="imageNames"></p>
    </div>

    <script type="text/javascript">
        $(".fileUpload input.upload").change(function () {
            console.log(this.files);
            
            var fileList = this.files;
            var text = "Files uploaded: ";
            for (var i = 0; i < fileList.length; i++) {
                var name = fileList[i].name;
                console.log(fileList[i]);
                text += "<p>" +name.substring(name.lastIndexOf('\\') + 1, name.length) + "</p>";
            }
            $('#imageNames').html(text);
        });
    </script>

</asp:Content>
