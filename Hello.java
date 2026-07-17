import javax.swing.JFrame;
import javax.swing.JLabel;
import javax.swing.SwingConstants;

public class Hello{

    public static void main(String[] args){

        JFrame janela=new JFrame("Hello World");

        JLabel texto=new JLabel("HELLO WORLD !!!");
        texto.setHorizontalAlignment(SwingConstants.CENTER);

        janela.add(texto);

        janela.setSize(800,600);
        janela.setDefaultCloseOperation(JFrame.EXIT_ON_CLOSE);
        janela.setLocationRelativeTo(null);
        janela.setVisible(true);

    }

}