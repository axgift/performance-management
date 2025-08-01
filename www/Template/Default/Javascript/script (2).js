$(document).ready(function() {
    // 应用程序主模块
    const WorkPlanApp = {
        // 初始化应用
        init: function() {
            this.bindEvents();
            this.loadInitialData();
            console.log('应用初始化完成');
            this.renderPerformanceTrendChart();

            // 添加窗口大小变化时重绘图表
            window.addEventListener('resize', function() {
                layui.use('chart', function() {
                    const chart = layui.chart;
                    chart.reload('performance-trend-chart');
                });
            });
        },

        // 渲染绩效趋势图表
        renderPerformanceTrendChart: function() {
        // 隐藏占位符
       $('.chart-placeholder').css('display', 'none');


        // 生成6个月的模拟绩效数据（实际应用中可替换为真实数据接口）
        const months = ['1月', '2月', '3月', '4月', '5月', '6月'];
        const scores = [85, 88, 90, 82, 89, 92];

        // 渲染折线图
        layui.use('chart', function() {
            const chart = layui.chart;
            const elem = $('#performance-trend-chart');

            const chartIns = chart.render({
                elem: elem,
                height: 300,
                type: 'line',
                data: {
                    labels: months,
                    datasets: [{
                        name: '绩效分数',
                        data: scores,
                        lineStyle: {
                            width: 3,
                            color: '#1E9FFF'
                        },
                        pointStyle: 'circle',
                        pointRadius: 6,
                        pointHoverRadius: 8,
                        label: {
                            show: true,
                            position: 'top'
                        }
                    }]
                },
                yAxis: {
                    min: 70,
                    max: 100,
                    tickInterval: 5,
                    grid: {
                        show: true
                    }
                },
                xAxis: {
                    grid: {
                        show: false
                    }
                },
                tooltip: {
                    trigger: 'axis',
                    formatter: function(params) {
                        return params[0].name + ': ' + params[0].value + '分';
                    }
                },
                legend: {
                    show: true,
                    position: 'bottom'
                },
                // 添加图表标题
                title: {
                    text: '个人绩效趋势',
                    left: 'center',
                    style: {
                        fontSize: 16,
                        fontWeight: 'bold'
                    }
                }
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
            
            // 计划相关
            $('.remove-item').on('click', this.handleRemovePlanlist.bind(this));
            $('#add-item').on('click', this.handleAddPlanlist.bind(this));


            $('#save-plan').on('click', this.handleSavePlan.bind(this));
            $('#preview-plan').on('click', this.handlePreviewPlan.bind(this));
            $(document).on('click', '.view-history', this.handleViewHistory.bind(this));
            
            // 总结相关
            $('#save-summary').on('click', this.handleSaveSummary.bind(this));
            $(document).on('click', '.view-details', this.handleViewDetails.bind(this));
            
            // 评分相关
            $('#save-score').on('click', this.handleSaveScore.bind(this));
            
            // 个人资料相关
            $('#save-profile').on('click', this.handleSaveProfile.bind(this));
            $('.toggle-password').on('click', this.togglePasswordVisibility.bind(this));
        },



        // 加载初始数据
        loadInitialData: function() {
            try {
                const currentDate = new Date();
                const currentWeek = this.getISOWeek(currentDate);
                $('#plan-week').val(currentWeek);
                $('#summary-week').val(currentWeek);

                $('.plan-item-select').empty();

                for(var i=0;i<7;i++)
                {
                    $(".plan-item-select").append($("<option>", {
                        value: this.getISOWeekDay(currentDate,i),
                        text: "周"+(i+1)
                    }));
                }
                

                


                this.loadSavedPlan(currentWeek);
                this.loadPlanHistory();
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

        // 加载已保存的计划
        loadSavedPlan: function(week) {
            try {
                
                const week=$("#plan-week").val();
               

                $.getJSON("/AjaxFun/post.ashx?r=getweekplanstatus&week=" + week + "&rs=" + Math.floor(Math.random() * 1000), function (data) {
                    if (unescape(data.code) == "0") {
                        $(".plan-form").show();
                    }else
                    {
                        $(".plan-form").hide();
                    }
                });
                

            } catch (error) {
                console.error('加载保存的计划失败:', error);
            }
        },

        // 加载计划历史
        loadPlanHistory: function() {
            try {
                const historyList = $('#plan-history-list');
                historyList.empty();

                // 从localStorage获取所有计划
                const plans = [];


                $.ajax({
                    type: "POST",
                    url: "/AjaxFun/post.ashx",
                    data: { r: "getweekplanlist"},
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
                                    plans.push(data.Datalist[i].Plan_ID);
                                }
                            }



                            // 按时间倒序排序
                            plans.sort().reverse();
                            // 显示最近3个计划
                            const recentPlans = plans.slice(0, 3);
                            recentPlans.forEach(week => {
                                const historyItem = $(`
                                    <div class="history-item">
                                        <span class="history-date">${week}</span>
                                        <button class="btn view-history" data-week="${week}">查看</button>
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
            const employeeName = $(e.currentTarget).text();
            $('.score-form h3').text(`评分 - ${employeeName} (2023-W44)`);
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
        // 删除计划条目
        handleRemovePlanlist: function(e) {
            try {
                const week = $('#plan-week').val();
                
                $(e.currentTarget).parent().remove();



            } catch (error) {
                console.error('保存计划失败:', error);
                this.showError('保存计划失败，请稍后重试');
            }
        },

        // 添加计划条目
        handleAddPlanlist: function() {
            try {

                var cloned = $($(".plan-item")[0]).clone(true);

                $(cloned).find(".plan-item-input").val("");
                //const week = $('#plan-week').val();
                
                //let tempHtml=" <div class=\"plan-item\">";
                //tempHtml+="            <input type=\"text\" class=\"plan-item-input\" placeholder=\"输入任务内容...\">";
                //tempHtml+="            <button type=\"button\" class=\"btn remove-item\">删除</button>";
                //tempHtml+="        </div>";

                cloned.appendTo("#plan-items-container");
                //$("#plan-items-container").append(tempHtml);

                //$("#plan-items-container").find(".remove-item").off().on('click', this.handleRemovePlanlist.bind(this));

            } catch (error) {
                console.error('保存计划失败:', error);
                this.showError('保存计划失败，请稍后重试');
            }
        },

        // 处理保存计划
        handleSavePlan: function() {
            try {
                const week = $('#plan-week').val();

                const currentDate = new Date();
                const planitemlist=$(".plan-item-input");
                const planitemdatalist=$(".plan-item-select");
                const lastday = this.getISOWeekLastDay(currentDate)

                if(planitemlist.length>0)
                {
                    
                    var plans=[];

                    for(var i=0;i<planitemlist.length;i++)
                    {
                        if($(planitemlist[i]).val().length>0)
                        {
                            plans.push($(planitemlist[i]).val()+"|"+$(planitemdatalist[i]).val());
                        }
                    }


                    var _this=this;
                    $.ajax({
                        type: "POST",
                        url: "/AjaxFun/post.ashx",
                        data: { r: "addweekplan",week:week,lastday:lastday,planlist:JSON.stringify(plans)},
                        datatype: "json",
                        beforeSend: function () {
                            $(".week_load").html("<div class=\"loadpan\"><img src=\"/images/loading.gif\" style=\"max-height:50px;\" /></div>");
                        },
                        success: function (_data) {
                            //            var returnStr = data.aeopFreightTemplateDTOList
                            //            alert(data.aeopFreightTemplateDTOList);
                            eval("var data =" + unescape(_data));

                            if (unescape(data.code) == "1") {
                                
                                _this.showSuccess(`周计划保存成功！\n周期: ${week}`);

                                var tempHtml="";

                                tempHtml+="<div class=\"plan-item\">";
                                tempHtml+="    <input type=\"text\" class=\"plan-item-input\" placeholder=\"输入任务内容...\">";
                                tempHtml+="    <button type=\"button\" class=\"btn remove-item\">删除</button>";
                                tempHtml+="</div>";

                                $("#plan-items-container").empty().html(tempHtml);
                                $(".week_load").html("");
                                _this.GoUrl();

                            } else {
                                layer.msg('获取失败,稍后重试.', { icon: 6 });
                            }
                            



                          


                        },
                        complete: function (XMLHttpRequest, textStatus) {
                            //                alert(XMLHttpRequest.responseText);
                            //                alert(textStatus);
                            $(".week_load").html("");
                        
                        },
                        error: function (XMLHttpRequest, textStatus) {
                            //alert(XMLHttpRequest.responseText);
                            $(".week_load").html("");
                        }
                    });








                }
                else
                {
                    return;
                }
                
             
            } catch (error) {
                console.error('保存计划失败:', error);
                this.showError('保存计划失败，请稍后重试');
            }
        },

        // 处理预览计划
        handlePreviewPlan: function() {
            try {
                const week = $('#plan-week').val();
                const content = $('#plan-content').val();

                if (!this.validatePlanContent(content)) {
                    return;
                }

                this.createPreviewWindow('周计划预览', week, content);
            } catch (error) {
                console.error('预览计划失败:', error);
                this.showError('预览计划失败，请稍后重试');
            }
        },

        // 处理查看历史计划
        handleViewHistory: function(e) {
            try {
                const week = $(e.currentTarget).data('week');
                var _this=this;

                $.ajax({
                    type: "POST",
                    url: "/AjaxFun/post.ashx",
                    data: { r: "getweekplanlists",week:week},
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

                            const content = localStorage.getItem(`weekly_plan_${week}`);


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
                                    plans.push(unescape(data.Datalist[i].Plan_Note));
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

                            _this.createPreviewWindow('历史计划查看', week, JSON.stringify(plans));




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
                console.error('查看历史计划失败:', error);
                this.showError('查看历史计划失败，请稍后重试');
            }
        },

        // 处理保存总结
        handleSaveSummary: function() {
            try {
                const week = $('#summary-week').val();
                const completed = $('#completed-tasks').val();
                const pending = $('#pending-tasks').val();
                const issues = $('#issues').val();

                if (!this.validateSummaryContent(completed)) {
                    return;
                }

                const summaryData = {
                    completed: completed,
                    pending: pending,
                    issues: issues,
                    submitted: new Date().toISOString()
                };

                localStorage.setItem(`weekly_summary_${week}`, JSON.stringify(summaryData));
                this.updateSummaryList(week);
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
                const summaryData = JSON.parse(localStorage.getItem(`weekly_summary_${week}`) || '{}');

                if (!summaryData.completed) {
                    this.showError('未找到该周总结数据');
                    return;
                }

                this.createSummaryDetailWindow(week, summaryData);
            } catch (error) {
                console.error('查看总结详情失败:', error);
                this.showError('查看总结详情失败，请稍后重试');
            }
        },

        // 处理保存评分
        handleSaveScore: function() {
            try {
                const score = $('#score').val();
                const comments = $('#comments').val();
                const employeeName = $('.employee-item.active').text();

                if (!this.validateScore(score)) {
                    return;
                }

                // 在实际应用中，这里会将评分保存到服务器
                this.showSuccess(`评分保存成功！\n员工: ${employeeName}\n评分: ${score}\n评语: ${comments || '无'}`);
            } catch (error) {
                console.error('保存评分失败:', error);
                this.showError('保存评分失败，请稍后重试');
            }
        },

        // 处理保存个人资料
        handleSaveProfile: function() {
            try {
                const email = $('#email').val();

                if (!this.validateEmail(email)) {
                    return;
                }

                // 在实际应用中，这里会将资料保存到服务器
                this.showSuccess('个人资料保存成功！');
            } catch (error) {
                console.error('保存个人资料失败:', error);
                this.showError('保存个人资料失败，请稍后重试');
            }
        },

        // 切换密码可见性
        togglePasswordVisibility: function() {
            const passwordInput = $('#password');
            const type = passwordInput.attr('type') === 'password' ? 'text' : 'password';
            passwordInput.attr('type', type);
            $('.toggle-password').text(type === 'password' ? '显示' : '隐藏');
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

        // 创建总结详情窗口
        createSummaryDetailWindow: function(week, data) {

            // 使用LAYUI的layer弹窗代替新窗口
            const formattedDate = this.formatDate(new Date(data.submitted));
            layui.use('layer', function() {
                const layer = layui.layer;
                const index = layer.open({
                    type: 1,
                    title: `工作总结详情 - ${week}`,
                    area: ['800px', '600px'],
                    shade: 0.3,
                    content: `
                        <div style="padding: 20px;">
                            <h1 style="color: #3498db; border-bottom: 2px solid #e0e0e0; padding-bottom: 10px;">周工作总结详情 (${week})</h1>
                            <p style="color: #777; font-size: 0.9rem; margin-bottom: 20px;">提交于：${formattedDate}</p>
                            <div style="margin: 20px 0;">
                                <h2 style="font-size: 1.3rem; color: #555; margin-bottom: 10px;">已完成任务</h2>
                                <div style="padding: 15px; background-color: #f9f9f9; border-radius: 4px; white-space: pre-wrap;">${data.completed || '无'}</div>
                            </div>
                            <div style="margin: 20px 0;">
                                <h2 style="font-size: 1.3rem; color: #555; margin-bottom: 10px;">未完成任务</h2>
                                <div style="padding: 15px; background-color: #f9f9f9; border-radius: 4px; white-space: pre-wrap;">${data.pending || '无'}</div>
                            </div>
                            <div style="margin: 20px 0;">
                                <h2 style="font-size: 1.3rem; color: #555; margin-bottom: 10px;">遇到的问题</h2>
                                <div style="padding: 15px; background-color: #f9f9f9; border-radius: 4px; white-space: pre-wrap;">${data.issues || '无'}</div>
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
            const year = date.getFullYear();
            const week = Math.ceil(((date - new Date(year, 0, 1)) / 86400000 + 1 + new Date(year, 0, 1).getDay()) / 7);
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