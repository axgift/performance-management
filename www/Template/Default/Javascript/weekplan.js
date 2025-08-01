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
                        
            // 计划相关
            $('.remove-item').on('click', this.handleRemovePlanlist.bind(this));
            $('#add-item').on('click', this.handleAddPlanlist.bind(this));


            $('#save-plan').on('click', this.handleSavePlan.bind(this));
            $('#preview-plan').on('click', this.handlePreviewPlan.bind(this));
            $(document).on('click', '.view-history', this.handleViewHistory.bind(this));

            $(document).on('click', '.update-btn',this.handleUpdateinfo.bind(this));
                  
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

                currentDate.setDate(currentDate.getDate() + 7);

                const currentWeek = this.getISOWeek(currentDate);

                

                $('#plan-week').val(currentWeek);


                $('.plan-item-select').empty();

                for(var i=0;i<7;i++)
                {
                    $(".plan-item-select").append($("<option>", {
                        value: this.getNextWeekDay(currentDate,i),
                        text: "周"+(i+1)
                    }));
                }                             

                if(this.isFridaySaturdaySunday())
                {
                    this.loadSavedPlan(currentWeek);
                }else
                {
                    //$(".plan-form").find(".form-group").remove();
                    //$(".plan-form").find(".form-actions").remove();
                    //$(".plan-form").find("h3").html("<center style=\"font-size:14px; color:red;\">周五后方可提交</center>");
                    $(".plan-form").hide();

                    $(".pan_arr").html("<center style=\"font-size:14px; color:red;\">周五后方可提交</center>").show();


                }

                this.loadPlanHistory();
            } catch (error) {
                console.error('加载初始数据失败:', error);
                this.showError('数据加载失败，请刷新页面重试');
            }
        },
        loadInitialDatabyedit: function() {
            try {
                const currentDate = new Date();
                const currentWeek = this.getISOWeek(currentDate);

                $('#plan-week').val(currentWeek);


                $('.plan-item-select').empty();

                for(var i=0;i<7;i++)
                {
                    $(".plan-item-select").append($("<option>", {
                        value: this.getNextWeekDay(currentDate,i),
                        text: "周"+(i+1)
                    }));
                }


               
                if(typeof($(".plan-item-select").attr("data-date"))=="string")
                {
                    const originalDate = $(".plan-item-select").attr("data-date");
                    const [datePart, timePart] = originalDate.split(' ');
                    const [year, month, day] = datePart.split('/');
                                        // 补零函数
                    const padZero = (num) => num.toString().padStart(2, '0');
                                        // 构建新格式
                    const formattedDate = `${year}-${padZero(month)}-${padZero(14)} ${timePart}`;                    

                    $(".plan-item-select").val(formattedDate);
                }

                
            } catch (error) {
                
            }
        },

        loadInitialWeekDaybyData: function(weekString) {
            $(".plan-item-select").empty();
           const [year, week] = weekString.split('-W').map(Number);

            // 步骤1: 找到年份的第一个星期四
            function getFirstThursday(year) {
                // 1月4日总是在第一个ISO周内
              const firstDay = new Date(year, 0, 4);
              const dayOfWeek = firstDay.getDay() || 7; // 转换为ISO星期(1=周一, 7=周日)
  
                // 计算第一个星期四：如果4号不是周四，则调整
              const firstThursday = new Date(firstDay);
                firstThursday.setDate(4 + (4 - dayOfWeek));
                return firstThursday;
            }

            // 步骤2: 计算目标周的周四
const firstThursday = getFirstThursday(year);
const targetThursday = new Date(firstThursday);
            targetThursday.setDate(firstThursday.getDate() + (week - 1) * 7);

            // 步骤3: 推算周一和周日
const monday = new Date(targetThursday);
            monday.setDate(targetThursday.getDate() - 3);

            // 生成一周的日期
const dates = Array.from({ length: 7 }, (_, i) => {
    const date = new Date(monday);
            date.setDate(monday.getDate() + i);
  
            // 格式化为 YYYY-MM-DD 23:59:59
  const y = date.getFullYear();
  const m = String(date.getMonth() + 1).padStart(2, '0');
  const d = String(date.getDate()).padStart(2, '0');
  
  $(".plan-item-select").append($("<option>", {
                value: `${y}-${m}-${d} 23:59:59`,
    text: "周"+(i+1)
    }));

            return `${y}-${m}-${d} 23:59:59`;
        });


            if($(".plan-item-select").length>0)
            {
                var planitemselect=$(".plan-item-select");
                for(var i=0;i<planitemselect.length;i++)
                {
                    if(typeof($(planitemselect[i]).attr("data-date"))=="string")
                    {
                      const originalDate = $(planitemselect[i]).attr("data-date");


                     const isoDateStr = originalDate.replace(/\//g, '-');
                    const dateObj = new Date(isoDateStr);

                                            // 提取日期组件并补零
                    const year = dateObj.getFullYear();
                    const month = String(dateObj.getMonth() + 1).padStart(2, '0');
                    const day = String(dateObj.getDate()).padStart(2, '0');

                                            // 组合新格式（保留原始时间部分）
                    const [, timePart] = originalDate.split(' ');
                    const formattedDate = `${year}-${month}-${day} ${timePart}`;
                        
                    $(planitemselect[i]).val(formattedDate);
                    }

                }


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
                                    plans.push({
                                        Plan_ID:data.Datalist[i].Plan_ID,
                                        Status:data.Datalist[i].status
                                    });
                                }
                            }



                            // 按时间倒序排序
                            plans.sort().reverse();
                            // 显示最近3个计划
                            const recentPlans = plans.slice(0, 10);
                            recentPlans.forEach(week => {
                                const historyItem = $(`
                                    <div class="history-item">
                                        <span class="history-date">${week.Plan_ID}</span>
                                        <button class="btn ${week.Status=='0'?'update-btn':'view-history'}" data-week="${week.Plan_ID}">${week.Status=='0'?'<font color="red">计划已驳回 重新编辑</font>':'查看'}</button>
                                    </div>
                    `);
                        historyList.append(historyItem);
                    });




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

        // 删除计划条目
        handleRemovePlanlist: function(e) {
            try {
                const week = $('#plan-week').val();
                
                if($(e.currentTarget).parent().parent().find(".plan-item").length>1)
                    $(e.currentTarget).parent().remove();
                else
                    this.showError('最少保存一条记录');



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
                const lastday = this.getNextWeekLastDay(currentDate)

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

        handleSavePlanUpdate: function() {
            try {
                const week = $('#plan-week').val();


                const currentDate = new Date();
                const planitemlist=$(".plan-item-input");
                const planitemdatalist=$(".plan-item-select");

                if(planitemlist.length>0)
                {
                    
                    var plans=[];

                    for(var i=0;i<planitemlist.length;i++)
                    {
                        if($(planitemlist[i]).val().length>0)
                        {
                            plans.push({
                                id:$(planitemlist[i]).attr("data-id"),
                                plan_note:$(planitemlist[i]).val(),
                                plan_date:$(planitemdatalist[i]).val()
                            });
                        }
                    }



                    var _this=this;
                    $.ajax({
                        type: "POST",
                        url: "/AjaxFun/post.ashx",
                        data: { r: "updateweekplan",week:week,planlist:JSON.stringify(plans)},
                        datatype: "json",
                        beforeSend: function () {
                            $(".week_load").html("<div class=\"loadpan\"><img src=\"/images/loading.gif\" style=\"max-height:50px;\" /></div>");
                        },
                        success: function (_data) {
                            //            var returnStr = data.aeopFreightTemplateDTOList
                            //            alert(data.aeopFreightTemplateDTOList);
                            eval("var data =" + unescape(_data));

                            if (unescape(data.code) == "1") {
                                
                                alert("修改成功");
                                
                                parent.document.location.href = window.location.href;

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

        handleUpdateinfo:function(e){
              const week = $(e.currentTarget).data('week');

              if ( confirm('确定重新编辑周计划?')) {
                  
                  
                  $("#plan-week").val(week);
                  var _this=this;
                  $("#plan-items-container").empty();
                  $.ajax({
                      type: "POST",
                      url: "/AjaxFun/post.ashx",
                      data: { r: "getweekplanlistsbyedit",week:week},
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
                                          Plan_Note:unescape(data.Datalist[i].Plan_Note),
                                          id:data.Datalist[i].id,
                                          Function_EndTime:data.Datalist[i].Function_EndTime
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

                              plans.sort().reverse();
                              const recentPlans = plans.slice(0, 20);

                              recentPlans.forEach(week => {
                                  const historyItem = $(`
                            <div class="plan-item">
                            <input type="text" class="plan-item-input" data-id="${week.id}" value="${week.Plan_Note}" placeholder="输入任务内容...">
                            <select id="n_week${week.id}" class="plan-item-select" data-date="${week.Function_EndTime}"></select>
                            <button type="button" class="btn remove-item">删除</button>
                        </div>
                    `);
                    $("#plan-items-container").append(historyItem);

                    _this.loadInitialWeekDaybyData($("#plan-week").val());
                    $('.remove-item').on('click', _this.handleRemovePlanlist.bind(_this));
                    $('#save-plan').off().on('click', _this.handleSavePlanUpdate.bind(this));
                      });
                              //_this.createPreviewWindow('历史计划查看', week, JSON.stringify(plans));

                      $(".pan_arr").empty().hide();
                      $(".plan-form").show();


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

                          //  const content = localStorage.getItem(`weekly_plan_${week}`);


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

                            if (!confirm("确定要标记为已完成吗？")) {
                                // 如果用户取消，直接返回，不执行后续操作
                                return;
                            }

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

        getNextWeekLastDay: function(date) {
            // 如果未传入日期，默认使用当前时间
            const currentDate = date || new Date();
    
            // 计算当前日期所在周的最后一天（周日）
            const dayOfWeek = currentDate.getDay();
            const firstDayOfWeek = new Date(currentDate);
            firstDayOfWeek.setDate(currentDate.getDate() - (dayOfWeek || 7) + 1); // 本周一
            let lastDayOfWeek = new Date(firstDayOfWeek);
            lastDayOfWeek.setDate(firstDayOfWeek.getDate() + 6); // 本周末（周日）
    
            // 计算下一周的最后一天（在本周末基础上加7天）
            const lastDayOfNextWeek = new Date(lastDayOfWeek);
            lastDayOfNextWeek.setDate(lastDayOfWeek.getDate() + 7);
    
            // 设置时间为当天结束（23:59:59.999）
            lastDayOfNextWeek.setHours(23, 59, 59, 999);
    
            // 格式化日期字符串
            return `${lastDayOfNextWeek.getFullYear()}-${
                String(lastDayOfNextWeek.getMonth() + 1).padStart(2, '0')}-${
                    String(lastDayOfNextWeek.getDate()).padStart(2, '0')} 23:59:59`;
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

        getNextWeekDay: function(date, num) {
            const year = date.getFullYear();
            // 计算当前日期是星期几（0是周日，6是周六）
            const dayOfWeek = date.getDay();
    
            // 计算本周的目标星期几（如num=0表示周一）
            const targetDayOfWeek = new Date(date);
            targetDayOfWeek.setDate(date.getDate() - (dayOfWeek || 7) + 1 + num);
    
            // 在此基础上加7天，得到下周的对应日期
            const nextWeekTargetDay = new Date(targetDayOfWeek);
            nextWeekTargetDay.setDate(targetDayOfWeek.getDate() + 0);
    
            // 设置时间为当天结束
            nextWeekTargetDay.setHours(23, 59, 59, 999);

            // 格式化日期字符串
            const formatted = `${nextWeekTargetDay.getFullYear()}-${
                String(nextWeekTargetDay.getMonth() + 1).padStart(2, '0')
            }-${
                String(nextWeekTargetDay.getDate()).padStart(2, '0')
            } 23:59:59`;
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