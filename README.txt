SWE204 Homework 1 LR Parser

Ad Soyad Abdulkadir Kılıç  
Öğrenci No B221202015  

Ad Soyad Nurseli Yıldız  
Öğrenci No B221202040  

Bu projede verilen grammar action ve goto tablolarına göre bir LR parser geliştirildi  
Amaç input dosyalarındaki ifadeleri shift reduce yöntemiyle parse etmeye yarar.  
Her bir input için bir trace tablosu ve bir parse tree oluşturulmaktadır  

Program C Sharp diliyle yazıldı ve dotnet 8 ile çalışıyor 
Program input klasöründeki input1txt den input9txt ye kadar olan dosyaları otomatik olarak işler  
Her input için output klasörüne outputXtxt ve logXtxt dosyaları oluşturur bu da outputs klasörüne kaydediliyor tam olarak hepsi.

Programı çalıştırmak için Proje klasörüne girin Visual Studio ile açın veya terminalden açın dotnet run komutunu yazın bu yeterlidir hocam...  

Programın kullandığı dosyalarda aşağıda tek tek .

Grammar txt gramer kurallarını içerir  
ActionTable txt shift reduce accept işlemlerinin tanımlı olduğu tablodur  
GotoTable txt nonterminal geçişlerini gösteren goto tablosudur  
inputs klasöründe input1txt ile input9txt arasında test dosyaları yer alır  
outputs klasöründe her input için trace ve parse tree içeren outputXtxt dosyaları bulunur  
outputs klasöründe aynı zamanda işlem adımlarını içeren logXtxt dosyaları da oluşur  
Program cs dosyası parser kodunun tamamını içerir  
Readme txt bu açıklama dosyasıdır  
