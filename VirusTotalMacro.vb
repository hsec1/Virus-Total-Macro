
Sub virusTotal()
Dim olItem As MailItem
Dim hyperlinks As Object
Dim baseUrls() As String
ReDim Preserve baseUrls(0)
'Get the email being inspected
Set olItem = GetCurrentItem()
'Get all hyperlinks in email
Set hyperlinks = olItem.GetInspector.WordEditor.hyperlinks
'The first two hyperlinks are always the mailto address
If hyperlinks.Count > 2 Then
    For Each link In hyperlinks
		Dim tmp As String
		tmp = link.Address
		If InStr(tmp, "mailto") = 0 Then
			Dim baseUrl As String
			baseUrl = getBaseUrl(tmp)
			ReDim Preserve baseUrls(UBound(baseUrls) + 1)
			baseUrls(UBound(baseUrls)) = baseUrl
		End If
    Next
	If UBound(baseUrls) < 1 Then
		MsgBox ("There are no Urls to check")
	Else
		getUniqueUrls (baseUrls)
	End If
Else
	MsgBox ("There are no Urls to check")
End If
End Sub

Private Function GetCurrentItem() As MailItem
Dim olMail As MailItem
    Set olMail = Application.ActiveExplorer.Selection.Item(1)
    If olMail Is Nothing Then
        Set olMail = Application.ActiveInspector
    End If
    Set GetCurrentItem = olMail
End Function

Function getBaseUrl(tmp As String) As String
Dim pos As Integer
Dim base As String
'Gets the base url and strips everything after a \. Begins at position 9 within the url
pos = InStr(9, tmp, "/")
If pos > 0 Then
    getBaseUrl = Left(tmp, pos - 1)
    End If
If pos = 0 And (InStr(tmp, "http") > 0 Or InStr(tmp, "www.")) Then
    getBaseUrl = tmp
    End If
End Function

Function getUniqueUrls(urls As Variant)
Dim d As Object
Dim i As Long
Dim hold As Integer
Dim choice As String
Set d = CreateObject("Scripting.Dictionary")
For i = LBound(urls) + 1 To UBound(urls)
    d(urls(i)) = 1
Next i
hold = UBound(d.keys()) - LBound(d.keys()) + 1

If hold > 7 Then
choice = InputBox("There are more than 7 Urls in this file to check. Continue (y/n)")
    If choice = "y" Then
        launchRequest (d.keys())
    End If
Else
  launchRequest (d.keys())
End If

End Function
Function launchRequest(uniUrls As Variant)
  Dim v As Variant
    For Each v In uniUrls
    apiCall (v)
    Next v
End Function


Function apiCall(suspUrl As String)
Dim api_key As String
Dim ya As Boolean
api_key = "<Your API KEY>"
Dim hReq As New WinHttpRequest
Dim fullUrl As String
fullUrl = "https://www.virustotal.com/vtapi/v2/url/report?apikey=" & api_key & "&resource=" & suspUrl
hReq.Open "GET", fullUrl
hReq.Send
If hReq.status = 404 Or hReq.status = 403 Then
    MsgBox ("Couldn't connect to virus total")
Else
   handleResponse hReq.ResponseText, suspUrl
End If
End Function

Function handleResponse(resp As String, url As String)
Dim splitter As Variant
Dim relInfo As String
splitter = split(resp, ",")
For Each spot In splitter
    If InStr(spot, "positives") > 0 Then
    relInfo = spot
    End If
Next
If relInfo = "" Then
MsgBox ("Virus Total has no data for the URL: " & url)
Else
MsgBox ("URL: " & url & vbCrLf & Replace(relInfo, """", ""))
End If
End Function
