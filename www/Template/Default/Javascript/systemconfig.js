$(document).ready(function() {
    
    if ($("#save_btn").length > 0) {
        $("#save_btn").on("click", function () {
            var webtitle = $("#systemtitle").val();
            var endpostday = $("#endpostday").val();
            var savedays = $("#savedays").val();



            $.ajax({
                type: "POST",
                url: "/AjaxFun/post.ashx",
                data: { r: "saveconfig", webtitle: webtitle, endpostday: endpostday, savedays: savedays },
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







});