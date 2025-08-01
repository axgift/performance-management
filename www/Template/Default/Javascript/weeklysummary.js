$(document).ready(function() {
    // 应用程序主模块
    const WorkPlanApp = {
        // 初始化应用
        init: function() {
            this.bindEvents();
            this.loadInitialData();
            console.log('应用初始化完成');

            // 添加窗口大小变化时重绘图表
            window.addEventListener('resize', function() {
                layui.use('chart', function() {
                    const chart = layui.chart;
                    chart.reload('performance-trend-chart');
                });
            });
        },
        
        // 绑定所有事件处理程序
        bindEvents: function() {
            // 导航切换
            $('nav ul li a').on('click', this.handleNavClick.bind(this));
                      
            var _this=this;
            $("#getSumary").on('click',function(){
                _this.loadWeekPlanlist($("#summary-week").val())
            });

            // 总结相关
            $('#save-summary').on('click', this.handleSaveSummary.bind(this));
            $(document).on('click', '.view-details', this.handleViewDetails.bind(this));
            
        },

        isFridaySaturdaySunday:function()
        {
            const today = new Date();
            const day = today.getDay();
            return day === 0 || day === 5 || day === 6;
        },

        // 加载初始数据
        loadInitialData: function() {
            try {

                const currentDate = new Date();
                const currentWeek = this.getISOWeek(currentDate);

  
                              
                if( $("#summary-week").length>0)
                {
                    $("#summary-week").val(currentWeek);
                }
                
                this.loadSummaryList();
                var _this=this;
                
                $.getJSON("/AjaxFun/post.ashx?r=getweeksummarytatus&week=" + currentWeek + "&rs=" + Math.floor(Math.random() * 1000), function (data) {

                    if (unescape(data.code) == "0") {

                        if(_this.isFridaySaturdaySunday())
                        {
                            _this.loadWeekPlanlist(currentWeek);
                            $(".summary-list-add").show();
                        }
                        else
                        {
                            
                            $(".summary-list-add").find(".form-group").remove();
                            $(".summary-list-add").find(".form-actions").remove();
                            $(".summary-list-add").find(".week-planlist").remove();
                            $(".summary-list-add").find("h3").html("<center style=\"font-size:14px; color:red;\">周五后方可提交</center>");
                            $(".summary-list-add").show();
                        }

                    }else
                    {
                        $(".summary-list-add").hide();
                    }
                });
                

            } catch (error) {
                console.error('加载初始数据失败:', error);
                this.showError('数据加载失败，请刷新页面重试');
            }
        },

        GoUrl:function(url)
        {
            if (typeof url != 'undefined' && !isNaN(parseInt(url.length)))
                parent.document.location.href = url;
            else
                parent.document.location.href = window.location.href;

        },


        // 加载计划历史
        loadSummaryList: function() {
            try {
                const summaryList = $('.summary-list');
                summaryList.empty();

                // 从localStorage获取所有计划
                const plans = [];

                $.ajax({
                    type: "POST",
                    url: "/AjaxFun/post.ashx",
                    data: { r: "getsummarylist"},
                    datatype: "json",
                    beforeSend: function () {
                        $(summaryList).html("<div class=\"loadpan\"><img src=\"/images/loading.gif\" style=\"max-height:50px;\" /></div>");
                    },
                    success: function (_data) {
                       
                        //            var returnStr = data.aeopFreightTemplateDTOList
                        //            alert(data.aeopFreightTemplateDTOList);
                        eval("var data =" + _data);
               

                        if (unescape(data.code) == "1") {
                            if(data.Datalist.length>0)
                            {
                                for(var i=0;i<data.Datalist.length;i++)
                                {
                                    plans.push({
                                        Plan_ID:data.Datalist[i].Plan_ID,
                                        Rating:data.Datalist[i].Rating,
                                        sumreason:data.Datalist[i].sumreason,
                                        reply:data.Datalist[i].reply,
                                        timestamp:data.Datalist[i].Timestamp
                                    });
                                }
                            }



                            // 按时间倒序排序
                            plans.sort().reverse();
                            // 显示最近3个计划
                            const recentPlans = plans.slice(0, 6);
                            recentPlans.forEach(week => {
                                const historyItem = $(`
<div class="summary-item">
    <div class="summary-header">
    <h3>${week.Plan_ID}</h3>
    ${week.Rating>0?'<div class="rating approved">'+week.Rating+'分</div>':'<div class="rating pending">等待评分</div>'}    
    </div>
    <p class="summary-date">提交于：${week.timestamp}</p>

    
    ${week.reply.length>2?'<div class="manager-comment"><strong>经理评语：'+ unescape(week.reply)+'</strong></div>':''}
    

    <button class="btn view-details">查看详情</button>
</div>
                    `);
                        summaryList.append(historyItem);
                    });




                        } 
                    },
                    complete: function (XMLHttpRequest, textStatus) {
                        //                alert(XMLHttpRequest.responseText);
                        //                alert(textStatus);
                        $(summaryList).find(".loadpan").remove();
                        
                    },
                    error: function (XMLHttpRequest, textStatus) {
                        //alert(XMLHttpRequest.responseText);
                        $(summaryList).find(".loadpan").remove();
                    }
                });



                              
               
            } catch (error) {
                console.error('加载计划历史失败:', error);
            }
        },

        loadWeekPlanlist:function(weekid){

            var plans=[];
            $.ajax({
                type: "POST",
                url: "/AjaxFun/post.ashx",
                data: { r: "getweekplanlistsbyzj",week:weekid},
                datatype: "json",
                beforeSend: function () {
                    $(".preview-list").html("<div class=\"loadpan\"><img src=\"/images/loading.gif\" style=\"max-height:50px;\" /></div>");
                },
                success: function (_data) {

                    //            var returnStr = data.aeopFreightTemplateDTOList
                    //            alert(data.aeopFreightTemplateDTOList);
                    eval("var data =" + _data);
                   

                    if (unescape(data.code) == "1") {
                        if(data.Datalist.length>0)
                        {
                            for(var i=0;i<data.Datalist.length;i++)
                            {
                                plans.push({
                                    id:data.Datalist[i].id,
                                    weekid: data.Datalist[i].Plan_ID,
                                    weekstatus: data.Datalist[i].status,
                                    weeknote: data.Datalist[i].Plan_Note
                                    
                                });
                            }
                        }


                        // 按时间倒序排序
                        plans.sort((a, b) => b.weekid - a.weekid);
                        // 显示最近3个计划
                        const recentPlans = plans.slice(0, 6);
                        recentPlans.forEach(week => {
                            // 在模板中通过 week.属性名 访问各个字段
                            const historyItem = $(`
                                <li><div style="float:left">${unescape(week.weeknote)}</div><div style="float:left;clear:left; margin:5px 0px 2px 100px; width:90%;"><textarea data-id="${week.id}" style="width:80%; padding:3px;" id="TextArea1" class="finput" placeholder="输入备注" cols="20" rows="2"></textarea></div></li>
                        `);

                        $(".week-planlist").find(".preview-list").append(historyItem);
                });


            const addsummary = $(`
                                <li><strong>周销售额</strong><input id="Weeklysales" class="w80" type="text" /><em>美金</em></li>
                                <li><strong>周广告支出</strong><input id="Weeklyadvertisingexpenditure" class="w80" type="text" /><em>美金</em></li>
                                <li><strong>订单数量</strong><input id="Numberoforders" class="w80" type="text" /></li>
                                <li><strong>售出商品数</strong><input id="Numberofitemssold" class="w80" type="text" /></li>
                        `);
                        $(".week-planlist").find(".preview-list").append(addsummary);


        } else {
                            layer.msg('获取失败,稍后重试.', { icon: 6 });
        }
        },
        complete: function (XMLHttpRequest, textStatus) {
            //                alert(XMLHttpRequest.responseText);
            //                alert(textStatus);
            $(".preview-list").find(".loadpan").remove();
                        
        },
        error: function (XMLHttpRequest, textStatus) {
            //alert(XMLHttpRequest.responseText);
            $(".preview-list").find(".loadpan").remove();
        }
        });





        },

        // 处理导航点击
        handleNavClick: function(e) {
            const targetId = $(e.currentTarget).attr('href').substring(1);

            // 更新导航激活状态
            $('nav ul li a').removeClass('active');
            $(e.currentTarget).addClass('active');

            // 更新内容区域显示
            $('.content-section').removeClass('active');
            $(`#${targetId}`).addClass('active');
        },

        // 处理保存总结
        handleSaveSummary: function() {
            try {
                const week = $('#summary-week').val();
                const issues = $('#issues').val();

                const Weeklysales= $('#Weeklysales').val();
                const Weeklyadvertisingexpenditure= $('#Weeklyadvertisingexpenditure').val();
                const Numberoforders =$('#Numberoforders').val();
                const Numberofitemssold= $('#Numberofitemssold').val();

                var inputlist=$(".finput");

                var summaryarr=[];

                for(var i=0;i<inputlist.length;i++)
                {
                   summaryarr.push({
                               id:$(inputlist[i]).attr("data-id"),
                               summarynote: $(inputlist[i]).val()                                  
                           });
                }
                    
                if(Weeklysales.length<1||Weeklyadvertisingexpenditure.length<1||Numberoforders.length<1||Numberofitemssold.length<1)
                {
                    
                    this.showError('销售信息必须填写.');
                    return;
                }

                const summaryData = {
                    week:week,
                    issues: issues,
                    submitted: new Date().toISOString(),
                    Weeklysales:Weeklysales,
                    Weeklyadvertisingexpenditure:Weeklyadvertisingexpenditure,
                    Numberoforders:Numberoforders,
                    Numberofitemssold:Numberofitemssold
                };

                var _this=this;
                $.ajax({
                    type: "POST",
                    url: "/AjaxFun/post.ashx",
                        data: { r: "addweeksummanr",summaryData:JSON.stringify(summaryData),listjson:JSON.stringify(summaryarr)},
                    datatype: "json",
                    beforeSend: function () {
                        //$("#plan-history-list").html("<div class=\"loadpan\"><img src=\"/images/loading.gif\" style=\"max-height:50px;\" /></div>");
                    },
                    success: function (_data) {
                        
                        //            var returnStr = data.aeopFreightTemplateDTOList
                        //            alert(data.aeopFreightTemplateDTOList);
                        eval("var data =" + _data);

                        const plans = [];
                        if (unescape(data.code) == "1") {

                          //  const content = localStorage.getItem(`weekly_plan_${week}`);
                            $(".summary-list-add").hide();

                        //var tempHtml="";
                        //tempHtml+="<div class=\"preview-container\" style=\"padding: 20px;\">";
                        //tempHtml+="    <h2 style=\"margin-bottom: 15px; color: #333;\">历史计划查看</h2>";
                        //tempHtml+="    <h3 style=\"margin-bottom: 20px; color: #666;\">周期: 2025-W26</h3>";
                        //tempHtml+="    <div class=\"preview-content\" style=\"max-height: 220px; overflow-y: auto;\">";
                        //if(data.Datalist.length>0)
                        //{
                        //    //tempHtml+="    <ul class=\"preview-list\">";

                        //    for(var i=0;i<data.Datalist.length;i++)
                        //    {
                        //        //tempHtml+="    <li>"+data.Datalist[i].Plan_Note+"</li>";
                        //        plans.push(unescape(data.Datalist[i].Plan_Note));
                        //    }                                
                        //    //tempHtml+="    </ul>";
                        //}
                        ////tempHtml+="    </div>";
                        ////tempHtml+="    <div style=\"text-align: center; margin-top: 20px;\">";
                        ////tempHtml+="        <button class=\"layui-btn layui-btn-primary preview-close-btn\">关闭</button>";
                        ////tempHtml+="    </div>";
                        ////tempHtml+="</div>";

                        //alert(JSON.stringify(plans));

                        //if (data.Datalist.length<1) {
                        //    _this.showError('未找到该周计划数据');
                        //    return;
                        //}
                        //alert(JSON.stringify(plans));
                        //_this.createPreviewWindow('历史计划查看', week, JSON.stringify(plans));




                    } else {
                           layer.msg('获取失败,稍后重试.', { icon: 6 });
            }
        },
        complete: function (XMLHttpRequest, textStatus) {
            //                alert(XMLHttpRequest.responseText);
            //                alert(textStatus);
            //$("#plan-history-list").find(".loadpan").remove();
                        
        },
        error: function (XMLHttpRequest, textStatus) {
            //alert(XMLHttpRequest.responseText);
            //$("#plan-history-list").find(".loadpan").remove();
        }
        });






















                this.loadSummaryList();
                this.clearSummaryForm();
                this.showSuccess(`工作总结提交成功！\n周期: ${week}`);


            } catch (error) {
                console.error('保存总结失败:', error);
                this.showError('保存总结失败，请稍后重试');
            }
        },

        // 处理查看详情
        handleViewDetails: function(e) {
            try {
                const week = $(e.currentTarget).closest('.summary-item').find('h3').text().split(' ')[0];

                const posttime = $(e.currentTarget).closest('.summary-item').find('.summary-date').text();
                var SUM_Reason="";
                var Weeklysales="";
                var Weeklyadvertisingexpenditure="";
                var Numberoforders="";
                var Numberofitemssold="";

                if (week.length<1) {
                    this.showError('未找到该周总结数据');
                    return;
                }

                var _this=this;


                $.ajax({
                    type: "POST",
                    url: "/AjaxFun/post.ashx",
                    data: { r: "getsummarylistsbyzj",week:week},
                    datatype: "json",
                    beforeSend: function () {
                        //$("#plan-history-list").html("<div class=\"loadpan\"><img src=\"/images/loading.gif\" style=\"max-height:50px;\" /></div>");
                    },
                    success: function (_data) {
                        
                        //            var returnStr = data.aeopFreightTemplateDTOList
                        //            alert(data.aeopFreightTemplateDTOList);
                        eval("var data =" + _data);

                        const plans = [];
                        if (unescape(data.code) == "1") {

                            //const content = localStorage.getItem(`weekly_plan_${week}`);
                            SUM_Reason= unescape(data.message).split('|')[0];
                            Weeklysales= unescape(data.message).split('|')[1];
                            Weeklyadvertisingexpenditure= unescape(data.message).split('|')[2];
                            Numberoforders= unescape(data.message).split('|')[3];
                            Numberofitemssold= unescape(data.message).split('|')[4];


                        //var tempHtml="";
                        //tempHtml+="<div class=\"preview-container\" style=\"padding: 20px;\">";
                        //tempHtml+="    <h2 style=\"margin-bottom: 15px; color: #333;\">历史计划查看</h2>";
                        //tempHtml+="    <h3 style=\"margin-bottom: 20px; color: #666;\">周期: 2025-W26</h3>";
                        //tempHtml+="    <div class=\"preview-content\" style=\"max-height: 220px; overflow-y: auto;\">";
                        if(data.Datalist.length>0)
                        {
                            //tempHtml+="    <ul class=\"preview-list\">";

                            for(var i=0;i<data.Datalist.length;i++)
                            {
                                //tempHtml+="    <li>"+data.Datalist[i].Plan_Note+"</li>";
                                plans.push({
                                    Plan_ID:data.Datalist[i].Plan_ID,
                                    status:data.Datalist[i].status,
                                    Timestamp:data.Datalist[i].Timestamp,
                                    Rating:data.Datalist[i].Rating,
                                    Admin_Reply:data.Datalist[i].Admin_Reply,
                                    Reply_timestamp:data.Datalist[i].Reply_timestamp,
                                    Plan_Note:unescape(data.Datalist[i].Plan_Note)
                                });
                            }                                
                            //tempHtml+="    </ul>";
                        }
                        //tempHtml+="    </div>";
                        //tempHtml+="    <div style=\"text-align: center; margin-top: 20px;\">";
                        //tempHtml+="        <button class=\"layui-btn layui-btn-primary preview-close-btn\">关闭</button>";
                        //tempHtml+="    </div>";
                        //tempHtml+="</div>";


                        if (data.Datalist.length<1) {
                            _this.showError('未找到该周计划数据');
                            return;
                        }

                        //_this.createPreviewWindow('历史计划查看', week, JSON.stringify(plans));
                        _this.createSummaryDetailWindow(week,posttime,SUM_Reason,Weeklysales,Weeklyadvertisingexpenditure,Numberoforders,Numberofitemssold,JSON.stringify(plans));



                    } else {
                            layer.msg('获取失败,稍后重试.', { icon: 6 });
            }
        },
        complete: function (XMLHttpRequest, textStatus) {
            //                alert(XMLHttpRequest.responseText);
            //                alert(textStatus);
            //$("#plan-history-list").find(".loadpan").remove();
                        
        },
        error: function (XMLHttpRequest, textStatus) {
            //alert(XMLHttpRequest.responseText);
            //$("#plan-history-list").find(".loadpan").remove();
        }
        });














               
            } catch (error) {
                console.error('查看总结详情失败:', error);
                this.showError('查看总结详情失败，请稍后重试');
            }
        },

        // 清空总结表单
        clearSummaryForm: function() {
            $('#issues').val('');
        },

        // 创建预览窗口
        createPreviewWindow: function(title, week, content) {
            // 如果content是字符串，尝试解析为数组（处理历史数据）
            let items = [];
            try {
                items = JSON.parse(content);
                if (!Array.isArray(items)) {
                    items = [content];
                }
            } catch (e) {
                items = [content];
            }
            
            // 构建列表HTML
            let listHtml = '<ul class="preview-list">';
            items.forEach(item => {
                if (item.trim()) {
                    listHtml += `<li>${item}</li>`;
                }
            });
            listHtml += '</ul>';

            const today = new Date();
            const year = today.getFullYear(); // 年份（4位数）
            const month = today.getMonth() + 1; // 月份（0-11，所以加1）
            const day = today.getDate(); // 日期（1-31）

            
            // 使用LAYUI的layer弹窗代替新窗口
            layui.use('layer', function() {
                const layer = layui.layer;
                const index = layer.open({
                    type: 1,
                    title: `${title} - ${week}`,
                   
                    shade: 0.3,
                    content: `
                        <div class="preview-container" style="padding: 20px;">
                            <h3 style="margin-bottom: 20px; color: #666;">周期: ${week} - 今天日期:${year}年${month}月${day}日</h3>
                            <div class="preview-content" style="max-height: 220px; overflow-y: auto;">${listHtml}</div>
                            <div style="text-align: center; margin-top: 20px;">
                                <button class="layui-btn layui-btn-primary preview-close-btn">关闭</button>
                            </div>
                        </div>
                    `,
                    success: function(layero) {
                        // 绑定关闭按钮事件
                        layero.find('.preview-close-btn').on('click', function() {
                            layer.close(index);
                        });
                    }
                });

                    if($(".sure_btn").length>0)
                    {
                        $(".sure_btn").on("click",function(){
                            var P_id=$(this).attr("data-id");
                            var _this=this;
                            $.getJSON("/AjaxFun/post.ashx?r=saveplanstatus&id=" + P_id + "&rs=" + Math.floor(Math.random() * 1000), function (data) {

                                if (unescape(data.code) == "1") {
                                    $(_this).parent().parent().append("<span class=\"sur_btn\">已完成</span>");
                                    $(_this).parent().remove();
                                }
                            });


                        });
                    }


            });
        },

        // 创建总结详情窗口
createSummaryDetailWindow: function(week, posttime,message,Weeklysales,Weeklyadvertisingexpenditure,Numberoforders,Numberofitemssold, content) {

            // 使用LAYUI的layer弹窗代替新窗口

            let items = [];
            try {
                items = JSON.parse(content);
                if (!Array.isArray(items)) {
                    items = [content];
                }
            } catch (e) {
                items = [content];
            }
            
            // 构建列表HTML
            let listHtml = '<ul class="preview-list">';
            items.forEach(item => {
                if (item.Plan_Note.trim()) {
                    listHtml += `<li>${item.Plan_Note}</li>`;
        }
        });


                    listHtml+="  <li><strong>周销售额</strong><input id=\"Weeklysales\" class=\"w80\" value=\""+Weeklysales+"\" type=\"text\" /><em>美金</em></li>";
                    listHtml+="  <li><strong>周广告支出</strong><input id=\"Weeklyadvertisingexpenditure\" value=\""+Weeklyadvertisingexpenditure+"\" class=\"w80\" type=\"text\" /><em>美金</em></li>";
                    listHtml+="  <li><strong>订单数量</strong><input id=\"Numberoforders\" value=\""+Numberoforders+"\" class=\"w80\" type=\"text\" /></li>";
                    listHtml+="  <li><strong>售出商品数</strong><input id=\"Numberofitemssold\" value=\""+Numberofitemssold+"\" class=\"w80\" type=\"text\" /></li>";

        listHtml += '</ul>';


            layui.use('layer', function() {
                const layer = layui.layer;
                const index = layer.open({
                    type: 1,
                    title: `工作总结详情 - ${week}`,

                    shade: 0.3,
                    content: `
                        <div style="padding: 20px;">
                            <h1 style="color: #3498db; border-bottom: 2px solid #e0e0e0; padding-bottom: 10px;">周工作总结详情 (${week})</h1>
                            <p style="color: #777; font-size: 0.9rem; margin-bottom: 20px;">${posttime}</p>
                            <div style="margin: 20px 0;">
                                ${listHtml}
                            </div>
                            <div style="margin: 20px 0;">
                                <h2 style="font-size: 1.3rem; color: #555; margin-bottom: 10px;">遇到的问题</h2>
                                <div style="padding: 15px; background-color: #f9f9f9; border-radius: 4px; white-space: pre-wrap;">${message}</div>
                            </div>
                            <div style="text-align: center; margin-top: 20px;">
                                <button class="layui-btn layui-btn-primary preview-close-btn">关闭</button>
                            </div>
                        </div>
                    `,
                    success: function(layero) {
                        // 绑定关闭按钮事件
                        layero.find('.preview-close-btn').on('click', function() {
                            layer.close(index);
                        });
                    }
                });
            });
        },

        // 验证计划内容
        validatePlanContent: function(content) {
            // 尝试解析内容为数组
            let items = [];
            try {
                items = JSON.parse(content);
                if (!Array.isArray(items)) {
                    items = [content];
                }
            } catch (e) {
                items = [content];
            }
            
            // 检查是否所有条目都为空
            const allEmpty = items.every(item => item.trim() === '');
            if (allEmpty) {
                this.showError('计划内容不能为空');
                return false;
            }
            
            // 检查单个条目长度
            for (let i = 0; i < items.length; i++) {
                if (items[i].trim().length > 200) {
                    this.showError(`第${i+1}个任务内容不能超过200个字符`);
                    return false;
                }
            }
            
            return true;
        },

        // 验证总结内容
        validateSummaryContent: function(completed) {
            if (!completed.trim()) {
                this.showError('请输入已完成任务');
                return false;
            }
            return true;
        },

        // 验证评分
        validateScore: function(score) {
            if (!score || score < 1 || score > 100) {
                this.showError('请输入有效的评分(1-100)');
                return false;
            }
            return true;
        },

        // 验证邮箱
        validateEmail: function(email) {
            if (!email.trim() || !this.isValidEmail(email)) {
                this.showError('请输入有效的邮箱地址');
                return false;
            }
            return true;
        },

        // 邮箱格式验证
        isValidEmail: function(email) {
            const re = /^[^\s@]+@[^\s@]+\.[^\s@]+$/;
            return re.test(email);
        },

        // 获取ISO格式的周数（YYYY-WW）
        getISOWeek: function(date) {
            const d = new Date(date);
            // 调整到当周星期四（关键修正：+4 - (d.getDay() || 7)）
            d.setDate(d.getDate() + (4 - (d.getDay() || 7)));
  
            const year = d.getFullYear();
  
            // 计算该年第一个星期四
            const firstThursday = new Date(year, 0, 4);
            firstThursday.setDate(4 - (firstThursday.getDay() || 7));
  
            // 计算周数差（关键修正：使用Math.floor而非Math.round）
            const msPerWeek = 7 * 24 * 60 * 60 * 1000;
            const weekDiff = Math.floor((d - firstThursday) / msPerWeek);
  
            // 计算ISO周数（周数差+1）
            let week = weekDiff + 1;
  
            // 处理跨年周的特殊情况（关键修正：判断是否属于上一年的最后一周）
            if (week < 1) {
                // 当前日期实际属于上一年的最后一周
              const prevYear = year - 1;
              const prevYearFirstThursday = new Date(prevYear, 0, 4);
                prevYearFirstThursday.setDate(4 - (prevYearFirstThursday.getDay() || 7));
              const prevYearWeeks = Math.floor((firstThursday - prevYearFirstThursday) / msPerWeek) + 1;
                return `${prevYear}-W${prevYearWeeks.toString().padStart(2, '0')}`;
            }
  
            // 处理年末可能属于下一年第一周的情况
            const nextYear = year + 1;
            const nextYearFirstThursday = new Date(nextYear, 0, 4);
            nextYearFirstThursday.setDate(4 - (nextYearFirstThursday.getDay() || 7));
            if (d >= nextYearFirstThursday) {
                return `${nextYear}-W01`;
            }

            return `${year}-W${week.toString().padStart(2, '0')}`;
        },

        getPreviousWeek: function(date) {
            // 创建当前日期的副本，避免修改原日期对象
            const previousDate = new Date(date);
            // 减去7天（7 * 24 * 60 * 60 * 1000毫秒）
            previousDate.setTime(previousDate.getTime() - 7 * 24 * 60 * 60 * 1000);
            // 调用现有方法获取上一周的周数
            return this.getISOWeek(previousDate);
        },

        getISOWeekLastDay:function(date)
        {
            const year = date.getFullYear();
            // 计算当前日期是当年的第几天
            const dayOfYear = Math.floor((date - new Date(year, 0, 1)) / 86400000) + 1;
            // 计算当年第一天是星期几（0是周日，6是周六）
            const firstDayOfYear = new Date(year, 0, 1).getDay();
            // 计算当前日期在本周的第几天（0是周日，6是周六）
            const dayOfWeek = date.getDay();
    
            // 计算本周第一天（周一）
            const firstDayOfWeek = new Date(date);
            firstDayOfWeek.setDate(date.getDate() - (dayOfWeek || 7) + 1);
    
            // 本周最后一天是第一天加6天（周日）
            const lastDayOfWeek = new Date(firstDayOfWeek);
            lastDayOfWeek.setDate(firstDayOfWeek.getDate() + 6);
    
            // 确保日期对象的时间部分为当天结束（23:59:59.999）
            lastDayOfWeek.setHours(23, 59, 59, 999);

            const formatted = `${lastDayOfWeek.getFullYear()}-${String(lastDayOfWeek.getMonth() + 1).padStart(2, '0')}-${String(lastDayOfWeek.getDate()).padStart(2, '0')} 23:59:59`;
            return formatted;
        },

        getISOWeekDay:function(date,num)
        {
                    const year = date.getFullYear();
            // 计算当前日期是当年的第几天
                    const dayOfYear = Math.floor((date - new Date(year, 0, 1)) / 86400000) + 1;
            // 计算当年第一天是星期几（0是周日，6是周六）
                    const firstDayOfYear = new Date(year, 0, 1).getDay();
            // 计算当前日期在本周的第几天（0是周日，6是周六）
                    const dayOfWeek = date.getDay();
    
            // 计算本周第一天（周一）
                    const firstDayOfWeek = new Date(date);
            firstDayOfWeek.setDate(date.getDate() - (dayOfWeek || 7) + 1);
    
            // 本周最后一天是第一天加6天（周日）
                    const lastDayOfWeek = new Date(firstDayOfWeek);
                    lastDayOfWeek.setDate(firstDayOfWeek.getDate() + num);
    
            // 确保日期对象的时间部分为当天结束（23:59:59.999）
            lastDayOfWeek.setHours(23, 59, 59, 999);

                    const formatted = `${lastDayOfWeek.getFullYear()}-${String(lastDayOfWeek.getMonth() + 1).padStart(2, '0')}-${String(lastDayOfWeek.getDate()).padStart(2, '0')} 23:59:59`;
            return formatted;
        },

        // 格式化日期
        formatDate: function(date) {
            return date.toLocaleDateString('zh-CN', {
                year: 'numeric',
                month: 'long',
                day: 'numeric'
            });
        },

        // 显示成功消息
        showSuccess: function(message) {
            layui.use('layer', function() {
                const layer = layui.layer;
                layer.msg(`成功：${message}`, {icon: 1, time: 2000});
            });
        },

        // 显示错误消息
        showError: function(message) {
            layui.use('layer', function() {
                const layer = layui.layer;
                layer.msg(`错误：${message}`, {icon: 2, time: 2000});
            });
        }
    };

    // 启动应用
    WorkPlanApp.init();
});