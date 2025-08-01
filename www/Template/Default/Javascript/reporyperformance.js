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



        // 加载初始数据
        loadInitialData: function() {
            try {
                const currentDate = new Date();

              //  currentDate.setDate(currentDate.getDate() - 7);

                const currentWeek = this.getISOWeek(currentDate);

                 const weekSelector = $('#weeknumlist');
                 weekSelector.empty(); // 清空初始选项

                for(var i=0;i<7;i++)
                {

                    // 计算i周前的日期（以本周一为基准）
                const today = new Date();
                    // 计算今天是星期几（0是星期日）
                const dayOfWeek = today.getDay() || 7; // 转换为1-7（1是星期一）
                    // 计算本周一的日期
                const monday = new Date(today);
                    monday.setDate(today.getDate() - (dayOfWeek - 1) - (i * 7));
                
                    // 计算该周的ISO周数和年份
                const year = this.getISOWeekYear(monday);
                const week = this.getISOWeek(monday);
                
                    // 格式化周数显示（与value保持一致）
                const weekValue = `${week}`;
                
                    // 添加到select选项（text和value保持一致）
                const option = $(`<option value="${weekValue}">`);
                    option.text(weekValue);
                    weekSelector.append(option);

                }      

                weekSelector.find('option:first').prop('selected', true);
                $('#plan-week').val(currentWeek);







                this.loadUserlist();

                const _this = this;
                $('#weeknumlist').on("change", function(e) {
                    // 调用 handleEmployeeSelect 方法，传入事件对象 e
                    // _this 指向 WorkPlanApp 实例，确保能访问到方法
                    _this.handleEmployeeSelect(e,"0");
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
handleEmployeeSelect: function(e,flag) {
    
    var userid=-1;
    if(typeof(flag)=="string")
    {
        userid=$("#userID").val();
    }else
    {
        $('.employee-item').removeClass('active');
        $(e.currentTarget).addClass('active');
            
        userid = $(e.currentTarget).attr("data-value");    
    }
            
            var _this=this;
            var plans=[];
            $(".preview-list").empty();
            $("#save-score").hide();
            $('#Weeklysales').html("0");
            $('#Weeklyadvertisingexpenditure').html("0");
            $('#Numberoforders').html("0");
            $('#Numberofitemssold').html("0");
            $('#totalscore').html("评分:0");

            week=$("#weeknumlist").val();

            $("#userID").val(userid);

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


                        //  $("#mainperformance h3").html("期数 ("+plans[0].Plan_ID+")");


                        $('#weeknumlist').val(plans[0].Plan_ID);
                        
                        // 按时间倒序排序
                        plans.sort().reverse();
                        
                        //显示最近3个计划
                        const recentPlans = plans.slice(0, 20);

                        recentPlans.forEach(week => {
                            const historyItem = $(`
                                <tr>
                                        <td><div class="d-note">${ unescape(week.Plan_Note)}</div></td>
                                        <td>${parseFloat(week.Rating)}</td>
                                        <td>${week.AdminReply}</td>
                                    </tr>                                
                        `);                                           
                            $(".preview-list").append(historyItem);
                    
                });


            $('#Weeklysales').html(data.Extdata.Weeklysales+" $");
            $('#Weeklyadvertisingexpenditure').html(data.Extdata.Weeklyadvertisingexpenditure+" $");
            $('#Numberoforders').html(data.Extdata.Numberoforders);
            $('#Numberofitemssold').html(data.Extdata.Numberofitemssold);
            $('#totalscore').html("评分:"+data.Extdata.Score);
            
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
                printDiv('printdiv');
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

        getISOWeekYear:function(date)
        {
            const tempDate = new Date(date.getTime());
            tempDate.setDate(tempDate.getDate() + 4 - (tempDate.getDay() || 7));
            return tempDate.getFullYear();
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




    // jQuery版本：打印指定ID的DIV内容
    function printDiv(divId) {
        // 使用jQuery选择器获取目标DIV
        const $divToPrint = $(`#${divId}`);
    
    // 检查目标DIV是否存在
    if ($divToPrint.length === 0) {
        alert('未找到指定的DIV元素');
        return;
    }

    // 创建打印窗口
    const printWindow = window.open('', '_blank');
    
    // 构建打印内容（包含样式）
    const printContent = `
        <html>
            <head>
                <title>打印内容</title>
                <!-- 引入原页面样式 -->
                 <link rel="stylesheet" href="/Template/default/CSS/styles.css?20418">
    <script src="/Javascript/jquery-3.6.0.min.js"></script>
    <link href="/Javascript/layui/v2.9.7/css/layui.css" rel="stylesheet" />
    <script src="/Javascript/layui/v2.9.7/layui.js"></script>
                <style>
                    @media print {
                        body { margin: 20px; font-family: Arial, sans-serif; background-color: white;}
        .no-print { display: none !important; }
    }
    </style>
</head>
<body style="background-color: white;">
    <h2 style="text-align: center; margin-bottom: 20px;">打印内容</h2>
    <!-- 目标DIV的完整内容（包括所有子元素和事件） -->
    ${$divToPrint.clone().html()}
                
    <div style="text-align: center; margin-top: 30px;" class="no-print">
        <button onclick="window.close()" class="bg-blue-500 text-white px-4 py-2 rounded">
            关闭打印预览
        </button>
    </div>
</body>
</html>
    `;

    // 写入并关闭文档
    $(printWindow.document.body).html(printContent);
    printWindow.document.close();

    // 等待窗口加载完成后打印
    $(printWindow).on('load', function() {
        printWindow.focus();
        setTimeout(() => {
            printWindow.print();
        // 可选：打印后自动关闭窗口
         printWindow.close();
    }, 500);
    });
    }