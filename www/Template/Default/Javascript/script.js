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
                       
        },



        // 加载初始数据
        loadInitialData: function() {
            try {
                const currentDate = new Date();
                const currentWeek = this.getISOWeek(currentDate);


                const lastdaydata=this.getISOWeekLastDay(currentDate);

                
                this.loadDefaultPlan(currentWeek,lastdaydata);
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

        loadDefaultPlan:function(week,lastdaydata){
            var todolist=[];
            var runstaff=[];
            var userpross=[];
            $.ajax({
                type: "POST",
                url: "/AjaxFun/post.ashx",
                data: { r: "getdefault",week:week},
                datatype: "json",
                beforeSend: function () {
                    $("#mainperformance").prepend("<div class=\"loadpan\"><img src=\"/images/loading.gif\" style=\"max-height:50px;\" /></div>");
                },
                success: function (_data) {
                    //            var returnStr = data.aeopFreightTemplateDTOList
                    //            alert(data.aeopFreightTemplateDTOList);
                    eval("var data =" + unescape(_data));

                    if (unescape(data.code) == "1") {

                        $("#tuandui").find(".progress-bar").css("width",""+((parseFloat(data.Datalist[0].GobalRunTaskNumber)/parseFloat(data.Datalist[0].GobalTaskNumber))*100).toFixed(2)+"%");
                        $("#tuandui").find("#tuandui-info").html("团队总体完成度: "+((parseFloat(data.Datalist[0].GobalRunTaskNumber)/parseFloat(data.Datalist[0].GobalTaskNumber))*100).toFixed(2)+"% (共"+data.Datalist[0].GobalTaskNumber+"项任务,已完成"+data.Datalist[0].GobalRunTaskNumber+"项)");

                        $("#geren").find(".progress-bar").css("width",""+((parseFloat(data.Datalist[0].StaffRunTaskNumber)/parseFloat(data.Datalist[0].StaffTaskNumber))*100).toFixed(2)+"%");
                        $("#geren").find("#geren-info").html("个人完成度: "+((parseFloat(data.Datalist[0].StaffRunTaskNumber)/parseFloat(data.Datalist[0].StaffTaskNumber))*100).toFixed(2)+"% (共"+data.Datalist[0].StaffTaskNumber+"项任务,已完成"+data.Datalist[0].StaffRunTaskNumber+"项)");
                      
                        $("#geren").find("#tuandui-status").html("已提交 ("+data.Datalist[0].GobalRunTaskNumber+"/"+data.Datalist[0].GobalTaskNumber+")");
                        ////待办事项
                        if(data.Datalist[0].waitdolist.length>0)
                        {
                            for(var i=0;i<data.Datalist[0].waitdolist.length;i++)
                            {
                                todolist.push(unescape(data.Datalist[0].waitdolist[i].Plan_Note));
                            }                                
                        }
                       
                        todolist.sort().reverse();
                          const recentPlans = todolist.slice(0, 10);

                        recentPlans.forEach(week => {
                            const historyItem = $(`
                                <li>${week}</li>
                              `);
                       
                        $("#geren").find(".todo-list").append(historyItem);

                    });
                    ////待办事项 结束

                    ////已提交
                    if(data.Datalist[0].RunTaskStafflist.length>0)
                    {
                        for(var i=0;i<data.Datalist[0].RunTaskStafflist.length;i++)
                        {
                            runstaff.push(unescape(data.Datalist[0].RunTaskStafflist[i].usernmae));
                        }                                
                    }


                    runstaff.sort().reverse();
                          const runstaffplans = runstaff.slice(0, 10);

                    runstaffplans.forEach(week => {
                        const historyItem = $(`
                            <li>${week}</li>
                        `);
                       
                    $("#geren").find(".staff-list").append(historyItem);

                });



            ////已提交结束


            ////团队员工进程
            if(data.Datalist[0].teamUserPross.length>0)
            {
                for(var i=0;i<data.Datalist[0].teamUserPross.length;i++)
                {
                    userpross.push({
                        username:unescape(data.Datalist[0].teamUserPross[i].staffid),
                        totaltasks:data.Datalist[0].teamUserPross[i].TotalTasks,
                        completedtasks:data.Datalist[0].teamUserPross[i].CompletedTasks
                    });
                }                                
            }

            
            userpross.sort().reverse();
                          const userprossfplans = userpross.slice(0, 10);

                          userprossfplans.forEach(week => {
                const historyItem = $(`
                    <div class="member-progress-item">
                          <span class="member-name">${week.username}</span>
                          <div class="progress-container">
                              <div class="progress-bar" style="width: ${(parseFloat(week.completedtasks) / parseFloat(week.totaltasks) * 100).toFixed(2)}%;"></div>
                          </div>
                          <span class="progress-percentage">${(parseFloat(week.completedtasks) / parseFloat(week.totaltasks) * 100).toFixed(2)}%</span>
                      </div>
                `);
                       
                $(".team-members-progress").find(".member-progress-list").append(historyItem);

        });


            ////团队员工进程结束











                    } else {
                        layer.msg('获取失败,稍后重试.', { icon: 6 });
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

$("#lastday").html("周计划提交截止："+lastdaydata);





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