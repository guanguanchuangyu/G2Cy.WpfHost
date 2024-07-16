# G2Cy.WpfHost

基于[WPF-构建容错复合应用程序](https://learn.microsoft.com/zh-cn/archive/msdn-magazine/2014/january/wpf-build-fault-tolerant-composite-applications)提供的源码案例，我改造了一个`.NET6`版本的案例。
使用技术如下：
`dotnetCampus.Ipc` 替代 `System.AddIn.Remoting` 中的进程间通信;
`HwndHost` 替代 `System.AddIn.Contract`;
目前基本能实现案例的加载x86以及x64位程序。

