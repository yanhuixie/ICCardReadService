# ICCardReadService
IC读卡Windows服务  
注意：目前只支持读取卡ID，其他功能请自行扩展。  
默认端口27813，请自行修改配置文件。  

## 调用方式
```javascript
  function geticcard(){
     $.ajax({
         async:false,
         type:"get",
         dataType:"json",
         url:"http://localhost:27813/read",
         success:function(data){
             var rs=eval(data);
             $("#iccardId").val(rs.cardId);
         },
         error: function (jqXHR, textStatus, errorThrown) {
             if(jqXHR.responseText!=null&&jqXHR.responseText!=""&&jqXHR.responseText!=undefined){
                 alert("读取IC卡出错："+jqXHR.responseText);
             }else{
                 alert("请先确认IC卡读卡器是否连接正常");
             }            
        }
     })
  }
```
