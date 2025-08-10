GRPC'yi exe olarak arka planda kapatamadım. Bunun için .proto uzantılı dosyayı değiştirmem gerekiyordu. Bana verilen proto dosyasını değiştirmek istemedim. Eğer böyle bir yetkim olsaydı proto dosyasına bir metot daha ekler arka planda çalışan ve channel.Shutdown() metodu ile kapanmayan o prosesi de kapatabilirdim.

Görev yöneticisinden gRPC için açılan Service.exe işlemini sonlandırabilirsiniz. Çok yük bindirmiyor ama yine de söylemek istedim.
