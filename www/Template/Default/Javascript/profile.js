$(document).ready(function() {
    
    if ($("#save-profile").length > 0) {
        $("#save-profile").on("click", function () {
            var email = $("#email").val();
            var password = $("#password").val();
            var id = $("#id").val();

            $.ajax({
                type: "POST",
                url: "/AjaxFun/post.ashx",
                data: { r: "saveuserinfo", email: email, password: password, id: id },
                datatype: "json",
                beforeSend: function () {
                    $("#loadpan").html("<img src=\"/images/loading.gif\" style=\"max-height:50px;\" />");
                },
                success: function (_data) {
                    //            var returnStr = data.aeopFreightTemplateDTOList
                    //            alert(data.aeopFreightTemplateDTOList);
                    eval("var data =" + unescape(_data));

                    if (unescape(data.code) == "1") {
                       
                        layer.msg('保存成功.', { icon: 1 });
                        
                    } else {
                        layer.msg('获取失败,稍后重试.', { icon: 6 });
                    }
                },
                complete: function (XMLHttpRequest, textStatus) {
                    //                alert(XMLHttpRequest.responseText);
                    //                alert(textStatus);
                    $("#loadpan").remove();

                },
                error: function (XMLHttpRequest, textStatus) {
                    //alert(XMLHttpRequest.responseText);
                    $("#loadpan").remove();
                }
            });



        });

    }



    $(".yellowbtn").on("click", function () {
        parent.document.location.href = "/logout";
    });



});


function GoUrl(url)
{
    if (typeof url != 'undefined' && !isNaN(parseInt(url.length)))
        parent.document.location.href = url;
    else
        parent.document.location.href = window.location.href;

}