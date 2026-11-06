// UseWPF 后 WindowsDesktop SDK 的隐式 using 集合不含 System.IO，这里显式补上
// （harness 大量使用 Path / File / Directory / SearchOption）。
global using System.IO;
