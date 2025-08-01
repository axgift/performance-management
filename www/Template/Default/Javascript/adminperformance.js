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
            
            // 员工选择（绩效评分页面）
            $('.employee-item').on('click', this.handleEmployeeSelect.bind(this));
            
            // 绩效标签切换
            $('.tab-btn').on('click', this.handleTabClick.bind(this));
                        
            
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

                if(this.isFridaySaturdaySunday())
                {
                }else
                {
                    currentDate.setDate(currentDate.getDate() - 7);
                }
               

                const currentWeek = this.getISOWeek(currentDate);


                $('#plan-week').val(currentWeek);


                this.loadUserlist();
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
        loadPerformance: function() {
            try {
                const historyList = $('#performance-history');
                historyList.empty();

                // 从localStorage获取所有计划
                const plans = [];


                $.ajax({
                    type: "POST",
                    url: "/AjaxFun/post.ashx",
                    data: { r: "getperformancelist"},
                    datatype: "json",
                    beforeSend: function () {
                        $("#plan-history-list").html("<div class=\"loadpan\"><img src=\"/images/loading.gif\" style=\"max-height:50px;\" /></div>");
                    },
                    success: function (_data) {
                        //            var returnStr = data.aeopFreightTemplateDTOList
                        //            alert(data.aeopFreightTemplateDTOList);
                        eval("var data =" + unescape(_data));

                        if (unescape(data.code) == "1") {
                            if(data.Datalist.length>0)
                            {
                                for(var i=0;i<data.Datalist.length;i++)
                                {
                                    plans.push({
                                        Plan_ID:data.Datalist[i].Plan_ID,
                                        Rating:data.Datalist[i].Rating
                                    });
                                }
                            }



                            // 按时间倒序排序
                            plans.sort().reverse();
                            // 显示最近3个计划
                            const recentPlans = plans.slice(0, 10);
                            recentPlans.forEach(week => {
                                const historyItem = $(`
                                    <div class="performance-item">
                                        <div class="period">${week.Plan_ID}</div>
                                        <div class="score">${week.Rating}分</div>
                                        <div class="rank"></div>
                                        <button class="btn view-details" data-id="${week.Plan_ID}">查看详情</button>
                                    </div>
                    `);
                        historyList.append(historyItem);
                    });




                        } else {
                            layer.msg('获取失败,稍后重试.', { icon: 6 });
                        }
                    },
                    complete: function (XMLHttpRequest, textStatus) {
                        //                alert(XMLHttpRequest.responseText);
                        //                alert(textStatus);
                        $("#plan-history-list").find(".loadpan").remove();
                        
                    },
                    error: function (XMLHttpRequest, textStatus) {
                        //alert(XMLHttpRequest.responseText);
                        $("#plan-history-list").find(".loadpan").remove();
                    }
                });



                              
               
            } catch (error) {
                console.error('加载计划历史失败:', error);
            }
        },
        
        loadUserlist:function(e){

            const userlist = $('#User_list');
            userlist.empty();

            const plans = [];
            var _this=this;

            $.ajax({
                type: "POST",
                url: "/AjaxFun/post.ashx",
                data: { r: "getuserlist"},
                datatype: "json",
                beforeSend: function () {
                    $("#User_list").html("<div class=\"loadpan\"><img src=\"/images/loading.gif\" style=\"max-height:50px;\" /></div>");
                },
                success: function (_data) {
                    //            var returnStr = data.aeopFreightTemplateDTOList
                    //            alert(data.aeopFreightTemplateDTOList);
                    eval("var data =" + unescape(_data));

                    if (unescape(data.code) == "1") {
                        if(data.Datalist.length>0)
                        {
                            for(var i=0;i<data.Datalist.length;i++)
                            {
                                plans.push({
                                    UserNmae:data.Datalist[i].UserNmae,
                                    ID:data.Datalist[i].ID
                                });
                            }
                        }



                        // 按时间倒序排序
                        plans.sort().reverse();
                        // 显示最近3个计划
                        const recentPlans = plans.slice(0, 10);
                        recentPlans.forEach(week => {
                            const historyItem = $(`
                                <li class="employee-item" data-value="${week.ID}">${week.UserNmae}</li>
                `);
                userlist.append(historyItem);


                });

            $('.employee-item').on('click', _this.handleEmployeeSelect.bind(this));
            $($('.employee-item')[0]).click();

        } else {
                            layer.msg('获取失败,稍后重试.', { icon: 6 });
        }
        },
        complete: function (XMLHttpRequest, textStatus) {
            //                alert(XMLHttpRequest.responseText);
            //                alert(textStatus);
            $("#User_list").find(".loadpan").remove();
                        
        },
        error: function (XMLHttpRequest, textStatus) {
            //alert(XMLHttpRequest.responseText);
            $("#User_list").find(".loadpan").remove();
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

        // 处理员工选择
        handleEmployeeSelect: function(e) {
            $('.employee-item').removeClass('active');
            $(e.currentTarget).addClass('active');
            
            const userid = $(e.currentTarget).attr("data-value");      
            var _this=this;
            var plans=[];
            $(".preview-list").empty();
            $(".preview-list").parent().find("per").remove();
            $("#save-score").hide();

            $.ajax({
                type: "POST",
                url: "/AjaxFun/post.ashx",
                data: { r: "getweekbyuserid",id:userid,week:$('#plan-week').val()},
                datatype: "json",
                beforeSend: function () {
                    $("#mainperformance").prepend("<div class=\"loadpan\"><img src=\"/images/loading.gif\" style=\"max-height:50px;\" /></div>");
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
                                for(var i=0;i<data.Datalist.length;i++)
                                {
                                    plans.push({
                                        id:data.Datalist[i].id,
                                        Plan_ID:data.Datalist[i].Plan_ID,
                                        Plan_Note:data.Datalist[i].Plan_Note,
                                        status:data.Datalist[i].status,
                                        Rating:data.Datalist[i].Rating,
                                        AdminReply:data.Datalist[i].Admin_Reply,
                                        Replytimestamp:data.Datalist[i].Reply_timestamp

                                    });
                                }
                            }

                            $("#save-score").show();
                        }

                        var returnMsg=unescape(data.message);


                        $("#mainperformance h3").html("期数 ("+plans[0].Plan_ID+")");
                        
                        
                        // 按时间倒序排序
                        plans.sort().reverse();
                        
                        //显示最近3个计划
                                const recentPlans = plans.slice(0, 20);

                        recentPlans.forEach(week => {
                            const historyItem = $(`
                                <li class="nole"><div style="float:left;${parseFloat(week.Rating)>0?'':'clear: right;'}">${ unescape(week.Plan_Note)}</div> <div style="float:left;${parseFloat(week.Rating)>0?'':'margin: 10px -10px;'}"><span class="reply_pan">${parseFloat(week.Rating)>0?week.Rating+' 分 回复:'+week.AdminReply+'</span>':'<input id="Text2" value="100" data-value="'+week.id+'" class="score" type="number" max="100" min="1" /> 分 <input id="Text2" value="" class="bnote" type="text" />'}</div>
                        `);


                   
                       
                    $(".preview-list").append(historyItem);
                    
                });

                     $(".preview-list").parent().append("<per>"+returnMsg+"</per>");
            
            $(".preview-list").find("input.score").on("change",function(){
                var davale=$(this).val();
                
                var scoreinput=$(".preview-list").find("input.score");
                var totalScoreNumber=0;
                if(scoreinput.length>0)
                {
                    for(var i=0;i<scoreinput.length;i++)
                    {
                        totalScoreNumber+=parseFloat($(scoreinput[i]).val());
                    }
                    $("#totalscore").html("总评分 ("+(totalScoreNumber/scoreinput.length).toFixed(2)+")");
                }
                

            });

                       
            $('#save-score').on('click', function(){
               if($(".preview-list").find("input.score").length>0)
                {
                var Scorelists=[];
               const scorelist=$(".preview-list").find("input.score");
                const scorebnote=$(".preview-list").find("input.bnote");
                var totalScroeNum=0;
                for(var i=0;i<scorelist.length;i++)
                   {
                    totalScroeNum+=parseFloat($(scorelist[i]).val());

                    Scorelists.push({
                                id:$(scorelist[i]).attr("data-value"),
                                score:$(scorelist[i]).val(),
                                bnote:$(scorebnote[i]).val()
                            });
                }
               

                $.getJSON("/AjaxFun/post.ashx?r=updaterating&scorelist=" + JSON.stringify(Scorelists) + "&rs=" + Math.floor(Math.random() * 1000), function (data) {
                    var returnStr = unescape(data.code);
                    if (returnStr == "1") {
                        layer.msg('评分成功.', { icon: 1 });

                            parent.document.location.href = window.location.href;

                    }
                });
               }else
               {
                   layer.msg('不能评分.', { icon: 6 });
               }
            });





                    } else {
                       // layer.msg('获取失败,稍后重试.', { icon: 6 });
                    }
                },
                complete: function (XMLHttpRequest, textStatus) {
                    //                alert(XMLHttpRequest.responseText);
                    //                alert(textStatus);
                    $("#mainperformance").find(".loadpan").remove();
                        
                },
                error: function (XMLHttpRequest, textStatus) {
                    //alert(XMLHttpRequest.responseText);
                    $("#mainperformance").find(".loadpan").remove();
                }
            });






























        },
        // 处理标签切换
        handleTabClick: function(e) {
            const tab = $(e.currentTarget).data('tab');

            // 更新标签激活状态
            $('.tab-btn').removeClass('active');
            $(e.currentTarget).addClass('active');

            // 更新内容区域显示
            $('.tab-content').removeClass('active');
            $(`#${tab}-performance`).addClass('active');
        },
        

        // 更新总结列表
        updateSummaryList: function(week) {
            const newSummaryItem = $(`
                <div class="summary-item">
                    <div class="summary-header">
                        <h3>${week} 工作总结</h3>
                        <div class="rating pending">等待评分</div>
                    </div>
                    <p class="summary-date">提交于：${this.formatDate(new Date())}</p>
                    <button class="btn view-details">查看详情</button>
                </div>
            `);
            $('.summary-list').prepend(newSummaryItem);
        },

        // 清空总结表单
        clearSummaryForm: function() {
            $('#completed-tasks').val('');
            $('#pending-tasks').val('');
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