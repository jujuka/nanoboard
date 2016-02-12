using System;
using System.Security.Cryptography;
using System.Text;
using System.Collections.Generic;
using System.IO;
using System.Windows.Forms;
using System.Drawing;
using System.Threading;
using System.Net.Sockets;
using System.Net;
using System.Linq;

namespace nboard
{
    class ImageBase64ConvertHandler : IRequestHandler
    {
        public NanoHttpResponse Handle(NanoHttpRequest request)
        {
            var sb = new StringBuilder();
            //ThreadViewHandler.AddHeader(sb);

            sb.Append("<br>Картинка -&gt; Base64:</br>");

			// Changes form's action on change of select
			sb.Append(@"<script>function change_action(select){
				var selected = select.options[select.selectedIndex].id;
				var form = document.getElementById(""upload-form"");
				if(selected == ""No""){
					form.action = ""convert"";
				} else {
					form.action = ""compress/"" + selected;
				}
			}</script>");

			// Default - "Умеренно". To change default: change action in form and change selected option
			sb.Append(@"<form id=""upload-form"" action=""compress/-70-80"" method=""post"" enctype=""multipart/form-data"">
				<input type=""file"" name=""file"" />
				<label for=""compression-level"">Степень сжатия: </label>
				<select id=""compression-level"" onchange=""change_action(this);"" >
					<option id=""No"">Без сжатия</option>
					<option id=""-90-90"">Незримо</option>
					<option id=""-80-85"">Слегка</option>
					<option id=""-70-80"" selected>Умеренно</option>
					<option id=""-60-75"">Весьма</option>
					<option id=""-50-70"">Нехило</option>
					<option id=""-45-65"">Ощутимо</option>
					<option id=""-40-60"">Основательно</option>
					<option id=""-35-55"">Конкретно</option>
					<option id=""-30-50"">Строго</option>
					<option id=""-25-45"">Безжалостно</option>
					<option id=""-20-40"">Бескомпромиссно</option>
					<option id=""-20-30"">Сильно</option>
					<option id=""-15-25"">Неистово</option>
					<option id=""-15-20"">Безумно</option>
					<option id=""-10-15"">Кошмарно</option>
					<option id=""-10-10"">Чудовищно</option>
					<option id=""-5-10"">Бессмысленно</option>
				</select>	
                <input type=""submit"" value=""convert"" />
            </form>");

            return new NanoHttpResponse(StatusCode.Ok, sb.ToString().ToNoStyleHtmlBody());
        }
    }
}